using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using dotenv.net;
using System.Threading.Tasks;

namespace AwesomeCopilot.Evaluation
{
    /// <summary>
    /// Prompt Evaluation Orchestrator - .NET Version
    ///
    /// This program helps orchestrate the evaluation of prompts, instructions, and chatmodes
    /// against different LLM models using GitHub Models. It provides utilities for:
    /// - Discovering evaluation targets
    /// - Organizing evaluation results
    /// - Generating reports
    /// - Managing evaluation workflows
    /// </summary>
    public class EvaluationOrchestrator
    {
        private readonly HttpClient _httpClient;
        private readonly string _githubToken;
        private readonly IChatClient _chatClient;

        // Configuration
        private readonly Dictionary<string, string> _directories = new()
        {
            { "prompts", "../prompts" },
            { "instructions", "../instructions" },
            { "chatmodes", "../chatmodes" }
        };

        private readonly Dictionary<string, string> _extensions = new()
        {
            { "prompts", ".prompt.md" },
            { "instructions", ".instructions.md" },
            { "chatmodes", ".chatmode.md" }
        };

        private readonly string _outputDir = "evaluation-results";

        private readonly string[] _models = {
            "GPT-4.1-mini",
            "Phi-4-mini-instruct",
            "Meta-Llama-3.1-8B-Instruct",
            "Mistral-Nemo"
        };

        private readonly string[] _metrics = {
            "accuracy",
            "relevance",
            "completeness",
            "clarity",
            "consistency",
            "response_time",
            "token_usage",
            "cost_efficiency"
        };

        private readonly string _baseUrl = "https://models.inference.ai.azure.com";

        public EvaluationOrchestrator()
        {
            _httpClient = new HttpClient();
            DotEnv.Load();
            _githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (string.IsNullOrEmpty(_githubToken))
            {
                Console.WriteLine("Warning: GITHUB_TOKEN not found in environment variable or .env file. GitHub Models API calls will fail.");
            }
            // Use Azure.AI.Inference and Microsoft.Extensions.AI for MEAI
            _chatClient = new ChatCompletionsClient(
                endpoint: new Uri(_baseUrl),
                new AzureKeyCredential(_githubToken)
            ).AsIChatClient(_models[0]); // Default to first model, can be changed per request
        }

        // Removed manual .env parsing; using dotenv.net instead

        public class EvaluationTarget
        {
            public string? Filename { get; set; }
            public string? Path { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Type { get; set; }
            public Dictionary<string, object>? Frontmatter { get; set; }
        }

        public class EvaluationPlan
        {
            public Dictionary<string, object>? Metadata { get; set; }
            public List<EvaluationMatrix>? EvaluationMatrix { get; set; }
            public List<string>? Recommendations { get; set; }
        }

        public class EvaluationMatrix
        {
            public EvaluationTarget? Target { get; set; }
            public string? Model { get; set; }
            public Dictionary<string, object>? Metrics { get; set; }
            public string? Status { get; set; }
            public List<string>? TestCases { get; set; }
        }

        private Dictionary<string, object> ExtractFrontmatter(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                var lines = content.Split('\n');

                if (lines.Length == 0 || lines[0] != "---")
                    return new Dictionary<string, object>();

                var frontmatter = new Dictionary<string, object>();
                var inFrontmatter = false;

                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (line == "---")
                    {
                        break;
                    }

                    if (i == 1)
                        inFrontmatter = true;

                    if (inFrontmatter)
                    {
                        var match = Regex.Match(line, @"^([^:]+):\s*(.*)$");
                        if (match.Success)
                        {
                            var key = match.Groups[1].Value.Trim();
                            var value = match.Groups[2].Value.Trim();

                            // Remove quotes if present
                            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                                (value.StartsWith("'") && value.EndsWith("'")))
                            {
                                value = value.Substring(1, value.Length - 2);
                            }

                            frontmatter[key] = value;
                        }
                    }
                }

                return frontmatter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to extract frontmatter from {filePath}: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        private string ExtractTitle(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                var lines = content.Split('\n');

                var inFrontmatter = false;
                var frontmatterEnded = false;

                foreach (var line in lines)
                {
                    if (line.Trim() == "---")
                    {
                        if (!inFrontmatter)
                        {
                            inFrontmatter = true;
                        }
                        else if (inFrontmatter && !frontmatterEnded)
                        {
                            frontmatterEnded = true;
                        }
                        continue;
                    }

                    if (frontmatterEnded && line.StartsWith("# "))
                    {
                        return line.Substring(2).Trim();
                    }
                }

                // Fallback to filename
                var basename = Path.GetFileNameWithoutExtension(filePath);
                return basename.Replace("-", " ").Replace("_", " ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to extract title from {filePath}: {ex.Message}");
                return "Unknown";
            }
        }

        public Dictionary<string, List<EvaluationTarget>> DiscoverEvaluationTargets()
        {
            var targets = new Dictionary<string, List<EvaluationTarget>>
            {
                { "prompts", new List<EvaluationTarget>() },
                { "instructions", new List<EvaluationTarget>() },
                { "chatmodes", new List<EvaluationTarget>() }
            };

            foreach (var kvp in _directories)
            {
                var type = kvp.Key;
                var directory = kvp.Value;

                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"Warning: Directory not found: {directory}");
                    continue;
                }

                var extension = _extensions[type];
                var files = Directory.GetFiles(directory, $"*{extension}");

                foreach (var file in files)
                {
                    var frontmatter = ExtractFrontmatter(file);
                    var title = ExtractTitle(file);

                    targets[type].Add(new EvaluationTarget
                    {
                        Filename = Path.GetFileName(file),
                        Path = file,
                        Title = title,
                        Description = frontmatter.ContainsKey("description") ? frontmatter["description"].ToString() : "No description available",
                        Type = type,
                        Frontmatter = frontmatter
                    });
                }
            }

            return targets;
        }

        public EvaluationPlan GenerateEvaluationPlan(Dictionary<string, List<EvaluationTarget>> targets)
        {
            var plan = new EvaluationPlan
            {
                Metadata = new Dictionary<string, object>
                {
                    { "generated", DateTime.Now.ToString("O") },
                    { "total_targets", 0 },
                    { "targets_by_type", new Dictionary<string, int>() }
                },
                EvaluationMatrix = new List<EvaluationMatrix>(),
                Recommendations = new List<string>()
            };

            // Calculate totals
            var totalTargets = 0;
            var targetsByType = new Dictionary<string, int>();

            foreach (var kvp in targets)
            {
                var count = kvp.Value.Count;
                targetsByType[kvp.Key] = count;
                totalTargets += count;
            }

            plan.Metadata["total_targets"] = totalTargets;
            plan.Metadata["targets_by_type"] = targetsByType;

            // Generate evaluation matrix
            foreach (var kvp in targets)
            {
                var type = kvp.Key;
                var items = kvp.Value;

                foreach (var item in items)
                {
                    foreach (var model in _models)
                    {
                        var metrics = new Dictionary<string, object>();
                        foreach (var metric in _metrics)
                        {
                            metrics[metric] = null;
                        }

                        plan.EvaluationMatrix.Add(new EvaluationMatrix
                        {
                            Target = item,
                            Model = model,
                            Metrics = metrics,
                            Status = "pending",
                            TestCases = new List<string>
                            {
                                "standard_use_case",
                                "edge_cases",
                                "context_variations",
                                "consistency_check"
                            }
                        });
                    }
                }
            }

            return plan;
        }

        public string CreateEvaluationReport(Dictionary<string, List<EvaluationTarget>> targets)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd");
            var totalFiles = targets.Values.Sum(v => v.Count);

            var report = new StringBuilder();
            report.AppendLine($"# Awesome Copilot Evaluation Report");
            report.AppendLine($"Generated: {DateTime.Now:O}");
            report.AppendLine();
            report.AppendLine("## Executive Summary");
            report.AppendLine();
            report.AppendLine("This report presents the evaluation results for prompts, instructions, and chatmodes in the awesome-copilot repository against multiple LLM models using GitHub Models.");
            report.AppendLine();
            report.AppendLine("### Evaluation Scope");
            report.AppendLine($"- **Total Files Evaluated**: {totalFiles}");
            report.AppendLine($"- **Prompts**: {targets["prompts"].Count}");
            report.AppendLine($"- **Instructions**: {targets["instructions"].Count}");
            report.AppendLine($"- **Chatmodes**: {targets["chatmodes"].Count}");
            report.AppendLine($"- **Models Tested**: {string.Join(", ", _models)}");
            report.AppendLine();
            report.AppendLine("### Key Findings");
            report.AppendLine("<!-- To be filled after evaluation -->");
            report.AppendLine("- **Best Overall Model**: TBD");
            report.AppendLine("- **Most Cost-Effective**: TBD");
            report.AppendLine("- **Fastest Response**: TBD");
            report.AppendLine("- **Most Consistent**: TBD");
            report.AppendLine();
            report.AppendLine("## Detailed Results");
            report.AppendLine();
            report.AppendLine("### Model Performance Overview");
            report.AppendLine("| Model | Avg Accuracy | Avg Relevance | Avg Completeness | Avg Clarity | Avg Consistency | Avg Cost | Avg Time |");
            report.AppendLine("|-------|--------------|---------------|------------------|-------------|-----------------|----------|----------|");

            foreach (var model in _models)
            {
                report.AppendLine($"| {model} | TBD | TBD | TBD | TBD | TBD | TBD | TBD |");
            }

            report.AppendLine();
            report.AppendLine("### Evaluation by Type");
            report.AppendLine();

            foreach (var kvp in targets)
            {
                var type = kvp.Key;
                var items = kvp.Value;

                report.AppendLine($"#### {char.ToUpper(type[0])}{type.Substring(1)} ({items.Count} files)");

                foreach (var item in items)
                {
                    report.AppendLine();
                    report.AppendLine($"##### {item.Title}");
                    report.AppendLine($"- **File**: `{item.Filename}`");
                    report.AppendLine($"- **Description**: {item.Description}");
                    report.AppendLine("- **Best Model**: TBD");
                    report.AppendLine("- **Overall Score**: TBD/10");
                    report.AppendLine("- **Recommendations**: TBD");
                }

                report.AppendLine();
            }

            report.AppendLine();
            report.AppendLine("## Recommendations");
            report.AppendLine();
            report.AppendLine("### Model Selection Guidelines");
            report.AppendLine("<!-- To be filled after evaluation -->");
            report.AppendLine("- **For Cost-Sensitive Projects**: TBD");
            report.AppendLine("- **For Maximum Quality**: TBD");
            report.AppendLine("- **For Balanced Performance**: TBD");
            report.AppendLine();
            report.AppendLine("### Optimization Opportunities");
            report.AppendLine("<!-- To be filled after evaluation -->");
            report.AppendLine("1. **High-Impact Improvements**: TBD");
            report.AppendLine("2. **Model-Specific Tuning**: TBD");
            report.AppendLine("3. **General Enhancements**: TBD");
            report.AppendLine();
            report.AppendLine("## Methodology");
            report.AppendLine();
            report.AppendLine("### Evaluation Framework");
            report.AppendLine($"- **Quality Metrics**: {string.Join(", ", _metrics.Where(m => !m.Contains("_")))}");
            report.AppendLine($"- **Performance Metrics**: {string.Join(", ", _metrics.Where(m => m.Contains("_")))}");
            report.AppendLine("- **Test Scenarios**: Standard use case, edge cases, context variations, consistency checks");
            report.AppendLine();
            report.AppendLine("### Test Configuration");
            report.AppendLine($"- **Models**: {string.Join(", ", _models)}");
            report.AppendLine($"- **GitHub Models API**: {_baseUrl}");
            report.AppendLine("- **Authentication**: GitHub Token (current user)");
            report.AppendLine("- **Temperature**: 0.1, 0.7, 1.0");
            report.AppendLine("- **Max Tokens**: 2000, 4000, 8000");
            report.AppendLine("- **Runs per Test**: 3 (for consistency measurement)");
            report.AppendLine();
            report.AppendLine("## Next Steps");
            report.AppendLine();
            report.AppendLine("1. **Implement Recommendations**: Apply high-impact improvements identified");
            report.AppendLine("2. **Monitor Performance**: Track changes in evaluation scores over time");
            report.AppendLine("3. **Expand Coverage**: Add new models and evaluation scenarios");
            report.AppendLine("4. **Automate Process**: Integrate evaluation into CI/CD pipeline");
            report.AppendLine();
            report.AppendLine("## Appendices");
            report.AppendLine();
            report.AppendLine("### A. Evaluation Criteria");
            report.AppendLine("[Detailed explanation of evaluation metrics and scoring methodology]");
            report.AppendLine();
            report.AppendLine("### B. Test Cases");
            report.AppendLine("[Complete list of test scenarios used for evaluation]");
            report.AppendLine();
            report.AppendLine("### C. Raw Data");
            report.AppendLine("[Link to detailed evaluation data and logs]");
            report.AppendLine();
            report.AppendLine("### D. GitHub Models Integration");
            report.AppendLine("[Details on GitHub Models API usage and authentication]");

            return report.ToString();
        }

        public async Task<bool> TestGitHubModelsConnection()
        {
            if (string.IsNullOrEmpty(_githubToken))
                return false;
            try
            {
                var response = await _chatClient.GetResponseAsync("Hello, this is a test message.");
                return !string.IsNullOrEmpty(response.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GitHub Models connection test failed: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> RunEvaluation(string targetFile, string model, string testPrompt)
        {
            if (string.IsNullOrEmpty(_githubToken))
                return new Dictionary<string, object> { { "error", "GitHub token not available" } };
            try
            {
                // Use the requested model for this evaluation
                var chatClient = new ChatCompletionsClient(
                    endpoint: new Uri(_baseUrl),
                    new AzureKeyCredential(_githubToken)
                ).AsIChatClient(model);
                var startTime = DateTime.Now;
                var response = await chatClient.GetResponseAsync(testPrompt);
                var endTime = DateTime.Now;
                return new Dictionary<string, object>
                {
                    { "success", true },
                    { "response", response.Text },
                    { "response_time", (endTime - startTime).TotalSeconds },
                    { "model", model },
                    { "target_file", targetFile }
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    { "success", false },
                    { "error", ex.Message }
                };
            }
        }

        public EvaluationTarget GetFileInfo(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                return null;
            }

            var frontmatter = ExtractFrontmatter(filePath);
            var title = ExtractTitle(filePath);
            var filename = Path.GetFileName(filePath);

            // Determine file type based on extension
            string type = "unknown";
            if (filename.EndsWith(".prompt.md"))
                type = "prompts";
            else if (filename.EndsWith(".instructions.md"))
                type = "instructions";
            else if (filename.EndsWith(".chatmode.md"))
                type = "chatmodes";

            return new EvaluationTarget
            {
                Filename = filename,
                Path = filePath,
                Title = title,
                Description = frontmatter.ContainsKey("description") ? frontmatter["description"].ToString() : "No description available",
                Type = type,
                Frontmatter = frontmatter
            };
        }

        public async Task<Dictionary<string, object>> EvaluateFile(string filePath, string model = null)
        {
            return await EvaluateFile(filePath, model, null);
        }

        public async Task<Dictionary<string, object>> EvaluateFile(string filePath, string model, string evaluatorPromptPath)
        {
            var target = GetFileInfo(filePath);
            if (target == null)
                return new Dictionary<string, object> { { "error", "File not found or invalid" } };

            // Load the evaluation prompt template
            string evalPromptTemplate = null;
            if (!string.IsNullOrEmpty(evaluatorPromptPath))
            {
                if (!File.Exists(evaluatorPromptPath))
                    return new Dictionary<string, object> { { "error", $"Evaluation prompt template not found: {evaluatorPromptPath}" } };
                evalPromptTemplate = File.ReadAllText(evaluatorPromptPath);
            }
            else
            {
                var defaultPromptPath = Path.Combine("..", "prompts", "evaluate-prompts-against-models.prompt.md");
                if (!File.Exists(defaultPromptPath))
                    return new Dictionary<string, object> { { "error", "Evaluation prompt template not found" } };
                evalPromptTemplate = File.ReadAllText(defaultPromptPath);
            }

            var modelsToTest = model != null ? new[] { model } : _models;
            var results = new Dictionary<string, object>
            {
                { "file", target },
                { "evaluations", new List<Dictionary<string, object>>() }
            };

            var evaluations = (List<Dictionary<string, object>>)results["evaluations"];

            foreach (var testModel in modelsToTest)
            {
                Console.WriteLine($"Evaluating {target.Filename} with model {testModel}...");

                // Inject the file content into the evaluation prompt template
                var fileContent = File.ReadAllText(filePath);
                var testPrompt = $"{evalPromptTemplate}\n\n---\n\n# Target File Content\n\n{fileContent}\n\n---\n\nPlease evaluate the above file according to the evaluation methodology.";

                var evaluation = await RunEvaluation(filePath, testModel, testPrompt);
                evaluation["model"] = testModel;
                evaluation["target"] = target.Filename ?? string.Empty;

                evaluations.Add(evaluation);
            }

            return results;
        }

        public async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            // New argument parsing: --action, --file, --model, --evaluatorPrompt
            string action = null;
            string file = null;
            string model = null;
            string evaluatorPrompt = null;

            foreach (var arg in args)
            {
                if (arg.StartsWith("--action="))
                    action = arg.Substring("--action=".Length).ToLower();
                else if (arg.StartsWith("--file="))
                    file = arg.Substring("--file=".Length);
                else if (arg.StartsWith("--model="))
                    model = arg.Substring("--model=".Length);
                else if (arg.StartsWith("--evaluatorPrompt="))
                    evaluatorPrompt = arg.Substring("--evaluatorPrompt=".Length);
            }

            // Fallback to positional for backward compatibility
            if (action == null && args.Length > 0)
                action = args[0].ToLower();
            if (file == null && args.Length > 1)
                file = args[1];
            if (model == null && args.Length > 2)
                model = args[2];

            switch (action)
            {
                case "discover":
                    Console.WriteLine("Discovering evaluation targets...");
                    var targets = DiscoverEvaluationTargets();
                    var json = JsonSerializer.Serialize(targets, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(json);
                    break;

                case "plan":
                    Console.WriteLine("Generating evaluation plan...");
                    var planTargets = DiscoverEvaluationTargets();
                    var plan = GenerateEvaluationPlan(planTargets);
                    Directory.CreateDirectory(_outputDir);
                    var planFile = Path.Combine(_outputDir, "evaluation-plan.json");
                    var planJson = JsonSerializer.Serialize(plan, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(planFile, planJson);
                    Console.WriteLine($"Evaluation plan generated: {planFile}");
                    Console.WriteLine($"Total evaluations planned: {plan.EvaluationMatrix?.Count ?? 0}");
                    break;

                case "report":
                    Console.WriteLine("Creating evaluation report template...");
                    var reportTargets = DiscoverEvaluationTargets();
                    var report = CreateEvaluationReport(reportTargets);
                    Directory.CreateDirectory(_outputDir);
                    var reportFile = Path.Combine(_outputDir, "evaluation-report.md");
                    await File.WriteAllTextAsync(reportFile, report);
                    Console.WriteLine($"Evaluation report template created: {reportFile}");
                    break;

                case "summary":
                    Console.WriteLine("Generating evaluation summary...");
                    var summaryTargets = DiscoverEvaluationTargets();
                    Console.WriteLine("\n=== EVALUATION SUMMARY ===");
                    var totalFiles = summaryTargets.Values.Sum(v => v.Count);
                    Console.WriteLine($"Total files: {totalFiles}");
                    Console.WriteLine($"- Prompts: {summaryTargets["prompts"].Count}");
                    Console.WriteLine($"- Instructions: {summaryTargets["instructions"].Count}");
                    Console.WriteLine($"- Chatmodes: {summaryTargets["chatmodes"].Count}");
                    Console.WriteLine($"Models to test: {_models.Length}");
                    Console.WriteLine($"Total evaluation combinations: {totalFiles * _models.Length}");
                    break;

                case "info":
                    if (string.IsNullOrEmpty(file))
                    {
                        Console.WriteLine("Error: Please provide a file path");
                        Console.WriteLine("Usage: dotnet run --action=info --file=<file-path>");
                        break;
                    }
                    var fileInfo = GetFileInfo(file);
                    if (fileInfo != null)
                    {
                        Console.WriteLine("\n=== FILE INFORMATION ===");
                        Console.WriteLine($"File: {fileInfo.Filename}");
                        Console.WriteLine($"Path: {fileInfo.Path}");
                        Console.WriteLine($"Title: {fileInfo.Title}");
                        Console.WriteLine($"Description: {fileInfo.Description}");
                        Console.WriteLine($"Type: {fileInfo.Type}");
                        Console.WriteLine($"Frontmatter: {JsonSerializer.Serialize(fileInfo.Frontmatter, new JsonSerializerOptions { WriteIndented = true })}");
                    }
                    break;

                case "evaluate":
                    if (string.IsNullOrEmpty(file))
                    {
                        Console.WriteLine("Error: Please provide a file path");
                        Console.WriteLine("Usage: dotnet run --action=evaluate --file=<file-path> [--model=<model>] [--evaluatorPrompt=<prompt-path>]");
                        break;
                    }
                    if (model != null && !_models.Contains(model))
                    {
                        Console.WriteLine($"Error: Unknown model '{model}'");
                        Console.WriteLine($"Available models: {string.Join(", ", _models)}");
                        break;
                    }
                    // Evaluator prompt selection logic
                    if (string.IsNullOrEmpty(evaluatorPrompt))
                    {
                        // Pick based on file type/location
                        var type = "";
                        if (file.EndsWith(".prompt.md")) type = "prompts";
                        else if (file.EndsWith(".instructions.md")) type = "instructions";
                        else if (file.EndsWith(".chatmode.md")) type = "chatmodes";

                        if (type == "prompts" || type == "instructions")
                            evaluatorPrompt = Path.Combine("..", "prompts", "evaluate-prompts-against-models.prompt.md");
                        else if (type == "chatmodes")
                            evaluatorPrompt = Path.Combine("..", "chatmodes", "prompt-evaluator.chatmode.md");
                        else
                            evaluatorPrompt = Path.Combine("..", "prompts", "evaluate-prompts-against-models.prompt.md");
                    }

                    Console.WriteLine($"Evaluating file: {file}");
                    Console.WriteLine($"Using evaluator prompt: {evaluatorPrompt}");
                    if (model != null)
                        Console.WriteLine($"Using model: {model}");
                    else
                        Console.WriteLine($"Using all models ({_models.Length} total)");

                    var evaluationResult = await EvaluateFile(file, model, evaluatorPrompt);
                    Directory.CreateDirectory(_outputDir);
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    var resultFile = Path.Combine(_outputDir, $"evaluation-result-{timestamp}.json");
                    var resultJson = JsonSerializer.Serialize(evaluationResult, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(resultFile, resultJson);
                    var htmlReportFile = Path.Combine(_outputDir, $"evaluation-result-{timestamp}.html");
                    var htmlReport = GenerateHtmlReport(evaluationResult);
                    await File.WriteAllTextAsync(htmlReportFile, htmlReport);
                    Console.WriteLine($"Evaluation completed. Results saved to: {resultFile}");
                    Console.WriteLine($"HTML report saved to: {htmlReportFile}");
                    break;

                case "test-connection":
                    Console.WriteLine("Testing GitHub Models connection...");
                    var connectionSuccessful = await TestGitHubModelsConnection();
                    if (connectionSuccessful)
                    {
                        Console.WriteLine("✓ GitHub Models connection successful");
                    }
                    else
                    {
                        Console.WriteLine("✗ GitHub Models connection failed");
                        Console.WriteLine("Please check your GITHUB_TOKEN environment variable");
                    }
                    break;

                default:
                    PrintHelp();
                    break;
            }
        }

        private string GenerateHtmlReport(Dictionary<string, object> evaluationResult)
        {
            var fileInfo = evaluationResult.ContainsKey("file") ? evaluationResult["file"] : null;
            var evaluations = evaluationResult.ContainsKey("evaluations") ? evaluationResult["evaluations"] as List<Dictionary<string, object>> : null;
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><title>Evaluation Report</title><style>body{font-family:sans-serif;}table{border-collapse:collapse;}th,td{border:1px solid #ccc;padding:6px;}th{background:#eee;}</style></head><body>");
            sb.AppendLine("<h1>Evaluation Report</h1>");
            if (fileInfo != null)
            {
                sb.AppendLine("<h2>File Information</h2><ul>");
                var fileDict = fileInfo as Dictionary<string, object>;
                if (fileDict != null)
                {
                    foreach (var kv in fileDict)
                    {
                        sb.AppendLine($"<li><b>{kv.Key}:</b> {kv.Value}</li>");
                    }
                }
                sb.AppendLine("</ul>");
            }
            if (evaluations != null)
            {
                sb.AppendLine("<h2>Evaluations</h2><table><tr><th>Model</th><th>Success</th><th>Response</th><th>Response Time (s)</th><th>Error</th></tr>");
                foreach (var eval in evaluations)
                {
                    var model = eval.ContainsKey("model") ? eval["model"] : "";
                    var success = eval.ContainsKey("success") ? eval["success"] : "";
                    var response = eval.ContainsKey("response") ? eval["response"] : "";
                    var responseTime = eval.ContainsKey("response_time") ? eval["response_time"] : "";
                    var error = eval.ContainsKey("error") ? eval["error"] : "";
                    sb.AppendLine($"<tr><td>{model}</td><td>{success}</td><td><pre style='max-width:600px;white-space:pre-wrap;'>{System.Net.WebUtility.HtmlEncode(response?.ToString() ?? "")}</pre></td><td>{responseTime}</td><td>{System.Net.WebUtility.HtmlEncode(error?.ToString() ?? "")}</td></tr>");
                }
                sb.AppendLine("</table>");
            }
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private void PrintHelp()
        {
            Console.WriteLine("Awesome Copilot Evaluation Orchestrator - .NET Version");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run <command> [arguments]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  discover              - Discover all evaluation targets");
            Console.WriteLine("  plan                  - Generate evaluation plan");
            Console.WriteLine("  report                - Create evaluation report template");
            Console.WriteLine("  summary               - Show evaluation summary");
            Console.WriteLine("  info <file-path>      - Show information about a specific file");
            Console.WriteLine("  evaluate <file-path> [model] - Evaluate a specific file against one or all models");
            Console.WriteLine("  test-connection       - Test GitHub Models API connection");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <file-path>           - Path to the file to evaluate (e.g., '../prompts/csharp-async.prompt.md')");
            Console.WriteLine("  [model]               - Optional specific model to use (e.g., 'gpt-4o-mini')");
            Console.WriteLine();
            Console.WriteLine("Environment Variables:");
            Console.WriteLine("  GITHUB_TOKEN          - GitHub personal access token for GitHub Models API");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run discover");
            Console.WriteLine("  dotnet run plan");
            Console.WriteLine("  dotnet run report");
            Console.WriteLine("  dotnet run summary");
            Console.WriteLine("  dotnet run info ../prompts/csharp-async.prompt.md");
            Console.WriteLine("  dotnet run evaluate ../prompts/csharp-async.prompt.md");
            Console.WriteLine("  dotnet run evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini");
            Console.WriteLine("  dotnet run test-connection");
            Console.WriteLine();
            Console.WriteLine("Available Models:");
            Console.WriteLine($"  {string.Join(", ", _models)}");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var orchestrator = new EvaluationOrchestrator();
            await orchestrator.Main(args);
            orchestrator.Dispose();
        }
    }
}
