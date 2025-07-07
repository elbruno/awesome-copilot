# Prompt Evaluation Framework

A comprehensive system for evaluating and comparing the effectiveness of prompts, instructions, and chatmodes against different LLM models using GitHub Models API with your GitHub personal access token.

## Overview

This evaluation framework provides systematic testing and comparison of the awesome-copilot repository's prompts, instructions, and chatmodes across multiple LLM models available through GitHub Models. It helps identify:

- **Best performing models** for specific use cases
- **Cost-effective solutions** for different scenarios
- **Optimization opportunities** for existing prompts
- **Performance benchmarks** and quality metrics

## Components

### 1. Evaluation Prompt (`prompts/evaluate-prompts-against-models.prompt.md`)
A comprehensive prompt that defines the evaluation methodology, metrics, and reporting format. Use this with GitHub Copilot to conduct systematic evaluations.

### 2. Prompt Evaluator Chatmode (`chatmodes/prompt-evaluator.chatmode.md`)
A specialized chatmode for conducting structured evaluations. Switch to this mode when you need to evaluate specific prompts or run comparative analyses.

### 3. Evaluation Orchestrator (`scripts/evaluate.js|py|cs`)
A multi-language script suite that automates the discovery, planning, and reporting of evaluations across the repository. Available in JavaScript (Node.js), Python, and .NET versions.

## Language Options

The evaluation framework is available in multiple programming languages to accommodate different development environments:

### JavaScript/Node.js (`evaluate.js`)
```bash
# Prerequisites: Node.js 16+ installed
node scripts/evaluate.js <command>
```

### Python (`evaluate.py`)
```bash
# Prerequisites: Python 3.8+ installed
python scripts/evaluate.py <command>
```

### .NET (`EvaluationOrchestrator.cs`)
```bash
# Prerequisites: .NET 8.0+ installed
dotnet run --project scripts/EvaluationOrchestrator.csproj <command>
```

All implementations provide the same functionality:
- Discover evaluation targets
- Generate evaluation plans 
- Create report templates
- Show evaluation summaries
- **NEW**: Get information about specific files
- **NEW**: Evaluate individual files against one or all models
- Test GitHub Models API connectivity
- Support for the same GitHub Models

## Quick Start

### Step 1: Setup GitHub Token

Set your GitHub personal access token to access GitHub Models:

**Windows:**
```cmd
set GITHUB_TOKEN=your_github_token_here
```

**Linux/macOS:**
```bash
export GITHUB_TOKEN=your_github_token_here
```

**Test the connection:**
```bash
# JavaScript/Node.js
node scripts/evaluate.js test-connection

# Python
python scripts/evaluate.py test-connection

# .NET
cd scripts && dotnet run test-connection
```

### Step 2: Generate Evaluation Overview

```bash
# Get a summary of what can be evaluated
node scripts/evaluate.js summary          # JavaScript
python scripts/evaluate.py summary        # Python  
cd scripts && dotnet run summary          # .NET
```

Example output:
```
=== EVALUATION SUMMARY ===
Total files: 67
- Prompts: 36
- Instructions: 22
- Chatmodes: 9
Models to test: 10
Total evaluation combinations: 670
```

### Step 3: Create Evaluation Plan

```bash
# Generate a detailed evaluation plan
node scripts/evaluate.js plan             # JavaScript
python scripts/evaluate.py plan           # Python
cd scripts && dotnet run plan             # .NET
```

This creates `evaluation-results/evaluation-plan.json` with a structured plan for testing all combinations.

### Step 4: Generate Report Template

```bash
# Create a report template
node scripts/evaluate.js report           # JavaScript
python scripts/evaluate.py report         # Python
cd scripts && dotnet run report           # .NET
```

This creates `evaluation-results/evaluation-report.md` with a structured template for documenting results.

## Command Reference

### Available Commands

All three implementations (JavaScript, Python, .NET) support the following commands:

```bash
# Get evaluation summary
<script> summary

# Discover all evaluation targets  
<script> discover

# Generate evaluation plan
<script> plan  

# Create report template
<script> report

# Get information about a specific file (NEW)
<script> info <file-path>

# Evaluate a specific file against all models (NEW)
<script> evaluate <file-path>

# Evaluate a specific file against a specific model (NEW)
<script> evaluate <file-path> <model-name>

# Test GitHub Models API connection
<script> test-connection
```

Where `<script>` is one of:
- `node scripts/evaluate.js` (JavaScript)
- `python scripts/evaluate.py` (Python)  
- `dotnet run --project scripts/EvaluationOrchestrator.csproj` (.NET)

### New Single-File Evaluation Commands

#### Get File Information
```bash
# Example: Get information about a specific prompt
node scripts/evaluate.js info ../prompts/csharp-async.prompt.md
python scripts/evaluate.py info ../prompts/csharp-async.prompt.md
dotnet run --project scripts/EvaluationOrchestrator.csproj info ../prompts/csharp-async.prompt.md
```

#### Evaluate Single File
```bash
# Example: Evaluate against all models
node scripts/evaluate.js evaluate ../prompts/csharp-async.prompt.md
python scripts/evaluate.py evaluate ../prompts/csharp-async.prompt.md
dotnet run --project scripts/EvaluationOrchestrator.csproj evaluate ../prompts/csharp-async.prompt.md

# Example: Evaluate against specific model
node scripts/evaluate.js evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini
python scripts/evaluate.py evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini
dotnet run --project scripts/EvaluationOrchestrator.csproj evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini
```

### Step 5: Conduct Evaluations

Use GitHub Copilot with the evaluation tools:

1. **For systematic evaluation**: Use the `evaluate-prompts-against-models` prompt
2. **For focused analysis**: Switch to the `prompt-evaluator` chatmode
3. **For specific prompts**: Run individual evaluations and document results

## Evaluation Methodology

### Models Tested

The framework supports evaluation against multiple LLM models available through GitHub Models:

- **GPT-4o-mini** - Cost-effective OpenAI model
- **GPT-4o** - High-quality OpenAI model  
- **Phi-3-mini-128k-instruct** - Microsoft compact model
- **Phi-3-medium-128k-instruct** - Microsoft medium model
- **Meta-Llama-3.1-70B-Instruct** - Meta large model
- **Meta-Llama-3.1-405B-Instruct** - Meta extra large model
- **Mistral-large** - Mistral AI large model
- **Mistral-Nemo** - Mistral AI compact model
- **Cohere-command-r** - Cohere standard model
- **Cohere-command-r-plus** - Cohere enhanced model

### Evaluation Metrics

#### Quality Metrics (1-10 scale)
- **Accuracy**: Correctness of generated output
- **Relevance**: Alignment with prompt requirements
- **Completeness**: Coverage of all requested aspects
- **Clarity**: Readability and understandability
- **Consistency**: Reproducibility across multiple runs

#### Performance Metrics
- **Response Time**: Average time to generate response
- **Token Usage**: Input/output token consumption
- **Cost Efficiency**: Cost per useful output token
- **Success Rate**: Percentage of successful completions

### Test Scenarios

1. **Standard Use Case**: Normal operation with typical inputs
2. **Edge Cases**: Boundary conditions and unusual inputs
3. **Context Variations**: Different project contexts and requirements
4. **Consistency Check**: Multiple runs to test reproducibility

## Usage Examples

### Example 1: Evaluate a Specific Prompt (NEW)

Using the evaluation orchestrator scripts to evaluate a single file:

```bash
# Get information about a specific file
node scripts/evaluate.js info ../prompts/csharp-async.prompt.md
python scripts/evaluate.py info ../prompts/csharp-async.prompt.md
dotnet run --project scripts/EvaluationOrchestrator.csproj info ../prompts/csharp-async.prompt.md

# Evaluate a specific file against all models
node scripts/evaluate.js evaluate ../prompts/csharp-async.prompt.md
python scripts/evaluate.py evaluate ../prompts/csharp-async.prompt.md
dotnet run --project scripts/EvaluationOrchestrator.csproj evaluate ../prompts/csharp-async.prompt.md

# Evaluate a specific file against a specific model
node scripts/evaluate.js evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini
python scripts/evaluate.py evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini
dotnet run --project scripts/EvaluationOrchestrator.csproj evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini
```

### Example 2: Using GitHub Copilot Chat

```markdown
I want to evaluate the `create-implementation-plan.prompt.md` against different models.

Please use the prompt-evaluator chatmode to:
1. Analyze the prompt structure and content
2. Test it against GPT-4o-mini, GPT-4o, and Phi-3-mini-128k-instruct
3. Compare response quality, speed, and cost
4. Provide optimization recommendations
```

### Example 3: Batch Evaluation

```markdown
Using the evaluate-prompts-against-models prompt, please:
1. Evaluate all C# related prompts and instructions
2. Compare performance across all supported models
3. Identify the most cost-effective model for C# development tasks
4. Generate a comparative report with recommendations
```

### Example 4: Model Selection

```markdown
I need to choose the best model for code review tasks. Please evaluate:
- All code review related prompts
- All relevant instructions (security, performance, etc.)
- The debug and refine-issue chatmodes

Compare models on accuracy, consistency, and cost for code review scenarios.
```

## Interpreting Results

### Model Performance Matrix

| Model | Accuracy | Relevance | Completeness | Clarity | Consistency | Cost | Time |
|-------|----------|-----------|--------------|---------|-------------|------|------|
| GPT-4o-mini | 8.5/10 | 9.0/10 | 8.8/10 | 9.2/10 | 8.5/10 | $0.015 | 2.3s |
| GPT-4o | 9.2/10 | 9.4/10 | 9.1/10 | 9.4/10 | 9.0/10 | $0.060 | 3.1s |
| Phi-3-mini-128k-instruct | 8.0/10 | 8.5/10 | 8.0/10 | 8.8/10 | 8.2/10 | $0.005 | 1.8s |

### Interpretation Guidelines

- **High Accuracy + High Consistency**: Reliable for production use
- **High Relevance + High Completeness**: Good for complex tasks
- **Low Cost + Good Performance**: Ideal for high-volume scenarios
- **Fast Response + Good Quality**: Suitable for interactive workflows

## Advanced Usage

### Custom Evaluation Criteria

You can extend the evaluation framework by:

1. **Adding new metrics**: Modify the evaluation prompts to include domain-specific metrics
2. **Custom test scenarios**: Create specific test cases for your use cases
3. **Specialized models**: Add evaluation for specialized or fine-tuned models
4. **Integration testing**: Test prompts in combination with real codebases

### Automated Evaluation

For continuous evaluation:

1. **CI/CD Integration**: Run evaluations when prompts are modified
2. **Scheduled Testing**: Regular evaluation to detect model drift
3. **Performance Monitoring**: Track evaluation metrics over time
4. **Cost Tracking**: Monitor usage costs across different models

## Best Practices

### Evaluation Planning
- Start with a representative sample of prompts
- Include both simple and complex scenarios
- Test across different domains and use cases
- Document assumptions and limitations

### Result Analysis
- Look for patterns across similar prompts
- Consider cost-benefit trade-offs
- Account for use case requirements
- Validate results with real-world usage

### Implementation
- Prioritize high-impact improvements
- Test optimizations incrementally
- Monitor changes in evaluation scores
- Share findings with the community

## Troubleshooting

### Common Issues

1. **API Rate Limits**: Implement delays between evaluations
2. **Cost Management**: Set budgets and monitor usage
3. **Model Availability**: Have fallback options for unavailable models
4. **Result Consistency**: Run multiple evaluations for statistical significance

### Performance Tips

- Use caching for repeated evaluations
- Batch similar requests together
- Optimize test case selection
- Parallelize independent evaluations

## Contributing

To improve the evaluation framework:

1. **Add new evaluation criteria**: Submit prompts with additional metrics
2. **Improve test scenarios**: Contribute better test cases
3. **Extend model support**: Add evaluation for new models
4. **Enhance reporting**: Improve result visualization and analysis

## Future Enhancements

- **Automated benchmarking**: Continuous evaluation pipeline
- **Model fine-tuning**: Evaluation-driven optimization
- **Community ratings**: Crowd-sourced evaluation data
- **Integration APIs**: Programmatic access to evaluation results

This framework provides a solid foundation for data-driven optimization of the awesome-copilot repository's prompts and instructions across different LLM models.