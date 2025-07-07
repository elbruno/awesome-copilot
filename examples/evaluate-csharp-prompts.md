# Example: Evaluating C# Prompts

This example demonstrates how to use the evaluation framework to compare C# development prompts against different LLM models.

## Step 1: Identify Evaluation Targets

First, let's see what C# related prompts we have:

```bash
cd scripts && dotnet run discover | grep -i "csharp\|dotnet\|\.net"
```

This will show us prompts like:
- `csharp-async.prompt.md`
- `csharp-docs.prompt.md`
- `csharp-mstest.prompt.md`
- `dotnet-best-practices.prompt.md`

## Step 2: Create Evaluation Plan

```bash
cd scripts && dotnet run plan
```

This generates a comprehensive evaluation plan in `evaluation-results/evaluation-plan.json`.

## Step 3: Conduct Evaluation

### Using the Evaluation Prompt

Use the `/evaluate-prompts-against-models` prompt with GitHub Copilot:

```
I want to evaluate C# development prompts against different models:

1. Target prompts:
   - csharp-async.prompt.md
   - csharp-docs.prompt.md
   - dotnet-best-practices.prompt.md

2. Test against:
   - GPT-4.1-mini
   - Phi-4-mini-instruct
   - Meta-Llama-3.1-8B-Instruct
   - Mistral-Nemo

3. Focus on:
   - Code quality and best practices
   - Documentation completeness
   - Performance considerations
   - Cost efficiency

Please provide a comparative analysis with specific recommendations.
```

### Using the Prompt Evaluator Chatmode

Switch to the `prompt-evaluator` chatmode and ask:

```
Please evaluate the csharp-async.prompt.md file:

1. Analyze the prompt structure and content quality
2. Test it with a sample async method scenario
3. Compare results between GPT-4.1-mini and GPT-4o
4. Provide optimization recommendations
```

## Step 4: Document Results

Use the generated report template:

```bash
cd scripts && dotnet run report
```

Then fill in the results from your evaluation.

## Expected Outcomes

You should be able to determine:

1. **Best Model for C# Tasks**: Which model provides the highest quality C# code
2. **Cost-Effective Option**: Which model offers the best value for money
3. **Prompt Optimizations**: How to improve existing C# prompts
4. **Usage Guidelines**: When to use which model for C# development

## Sample Results

Based on typical evaluations, you might find:

- **GPT-4.1-mini**: Good balance of quality and cost
- **Phi-4-mini-instruct**: Excellent compact model performance
- **Meta-Llama-3.1-8B-Instruct**: Cost-effective alternative with solid performance
- **Mistral-Nemo**: Strong performance for specific C# scenarios

This data helps you make informed decisions about which model to use for different C# development scenarios.