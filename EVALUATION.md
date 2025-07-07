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

### 3. Evaluation Orchestrator (`scripts/EvaluationOrchestrator.cs`)
A .NET application that automates the discovery, planning, and reporting of evaluations across the repository. The orchestrator provides:
- Discover evaluation targets
- Generate evaluation plans 
- Create report templates
- Show evaluation summaries
- Get information about specific files
- Evaluate individual files against one or all models
- Test GitHub Models API connectivity
- Support for GitHub Models

## Quick Start

### Step 1: Setup GitHub Token

Set your GitHub personal access token to access GitHub Models. You can do this via environment variable or .env file:

**Option 1: Environment Variable**

*Windows:*
```cmd
set GITHUB_TOKEN=your_github_token_here
```

*Linux/macOS:*
```bash
export GITHUB_TOKEN=your_github_token_here
```

**Option 2: .env File (Recommended)**

1. Copy the sample .env file:
```bash
cp scripts/.env.sample scripts/.env
```

2. Edit the `.env` file and replace `your_github_token_here` with your actual GitHub personal access token:
```
GITHUB_TOKEN=ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

3. Make sure to keep the `.env` file private and do not commit it to version control.

**Test the connection:**
```bash
# Prerequisites: .NET 8.0+ installed
cd scripts && dotnet run test-connection
```

### Step 2: Generate Evaluation Overview

```bash
# Get a summary of what can be evaluated
cd scripts && dotnet run summary
```

Example output:
```
=== EVALUATION SUMMARY ===
Total files: 67
- Prompts: 36
- Instructions: 22
- Chatmodes: 9
Models to test: 4
Total evaluation combinations: 268
```

### Step 3: Create Evaluation Plan

```bash
# Generate a detailed evaluation plan
cd scripts && dotnet run plan
```
cd scripts && dotnet run plan

This creates `evaluation-results/evaluation-plan.json` with a structured plan for testing all combinations.

### Step 4: Generate Report Template

```bash
# Create a report template
cd scripts && dotnet run report
```

This creates `evaluation-results/evaluation-report.md` with a structured template for documenting results.

## Command Reference

### Available Commands

The .NET implementation supports the following commands:

```bash
# Get evaluation summary
cd scripts && dotnet run summary

# Discover all evaluation targets  
cd scripts && dotnet run discover

# Generate evaluation plan
cd scripts && dotnet run plan

# Create report template
cd scripts && dotnet run report

# Get information about a specific file
cd scripts && dotnet run info <file-path>

# Evaluate a specific file against all models
cd scripts && dotnet run evaluate <file-path>

# Evaluate a specific file against a specific model
cd scripts && dotnet run evaluate <file-path> <model-name>

# Test GitHub Models API connection
cd scripts && dotnet run test-connection
```

### Single-File Evaluation Commands

#### Get File Information
```bash
# Example: Get information about a specific prompt
cd scripts && dotnet run info ../prompts/csharp-async.prompt.md
```

#### Evaluate Single File
```bash
# Example: Evaluate against all models
cd scripts && dotnet run evaluate ../prompts/csharp-async.prompt.md

# Example: Evaluate against specific model
cd scripts && dotnet run evaluate ../prompts/csharp-async.prompt.md GPT-4.1-mini
```

### Step 5: Conduct Evaluations

Use GitHub Copilot with the evaluation tools:

1. **For systematic evaluation**: Use the `evaluate-prompts-against-models` prompt
2. **For focused analysis**: Switch to the `prompt-evaluator` chatmode
3. **For specific prompts**: Run individual evaluations and document results

## Evaluation Methodology

### Models Tested

The framework supports evaluation against the following LLM models available through GitHub Models:

- **GPT-4.1-mini** - Latest cost-effective OpenAI model
- **Phi-4-mini-instruct** - Microsoft's newest compact model
- **Meta-Llama-3.1-8B-Instruct** - Meta's compact model
- **Mistral-Nemo** - Mistral AI's compact model

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

### Example 1: Evaluate a Specific Prompt

Using the evaluation orchestrator to evaluate a single file:

```bash
# Get information about a specific file
cd scripts && dotnet run info ../prompts/csharp-async.prompt.md

# Evaluate a specific file against all models
cd scripts && dotnet run evaluate ../prompts/csharp-async.prompt.md

# Evaluate a specific file against a specific model
cd scripts && dotnet run evaluate ../prompts/csharp-async.prompt.md GPT-4.1-mini
```

### Example 2: Using GitHub Copilot Chat

```markdown
I want to evaluate the `create-implementation-plan.prompt.md` against different models.

Please use the prompt-evaluator chatmode to:
1. Analyze the prompt structure and content
2. Test it against GPT-4o-mini, GPT-4o, and Phi-4-mini-instruct
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
| GPT-4.1-mini | 8.5/10 | 9.0/10 | 8.8/10 | 9.2/10 | 8.5/10 | $0.015 | 2.3s |
| Phi-4-mini-instruct | 8.0/10 | 8.5/10 | 8.0/10 | 8.8/10 | 8.2/10 | $0.005 | 1.8s |
| Meta-Llama-3.1-8B-Instruct | 8.2/10 | 8.7/10 | 8.3/10 | 8.6/10 | 8.4/10 | $0.008 | 2.0s |
| Mistral-Nemo | 8.1/10 | 8.6/10 | 8.2/10 | 8.5/10 | 8.3/10 | $0.007 | 1.9s |

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