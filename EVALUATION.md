# Prompt Evaluation Framework

A comprehensive system for evaluating and comparing the effectiveness of prompts, instructions, and chatmodes against different LLM models like GPT-4.1-mini, GPT-4, Claude, and others.

## Overview

This evaluation framework provides systematic testing and comparison of the awesome-copilot repository's prompts, instructions, and chatmodes across multiple LLM models. It helps identify:

- **Best performing models** for specific use cases
- **Cost-effective solutions** for different scenarios
- **Optimization opportunities** for existing prompts
- **Performance benchmarks** and quality metrics

## Components

### 1. Evaluation Prompt (`prompts/evaluate-prompts-against-models.prompt.md`)
A comprehensive prompt that defines the evaluation methodology, metrics, and reporting format. Use this with GitHub Copilot to conduct systematic evaluations.

### 2. Prompt Evaluator Chatmode (`chatmodes/prompt-evaluator.chatmode.md`)
A specialized chatmode for conducting structured evaluations. Switch to this mode when you need to evaluate specific prompts or run comparative analyses.

### 3. Evaluation Orchestrator (`scripts/evaluate.js`)
A Node.js script that automates the discovery, planning, and reporting of evaluations across the repository.

## Quick Start

### Step 1: Generate Evaluation Overview

```bash
# Get a summary of what can be evaluated
node scripts/evaluate.js summary
```

Example output:
```
=== EVALUATION SUMMARY ===
Total files: 67
- Prompts: 36
- Instructions: 22
- Chatmodes: 9
Models to test: 5
Total evaluation combinations: 335
```

### Step 2: Create Evaluation Plan

```bash
# Generate a detailed evaluation plan
node scripts/evaluate.js plan
```

This creates `evaluation-results/evaluation-plan.json` with a structured plan for testing all combinations.

### Step 3: Generate Report Template

```bash
# Create a report template
node scripts/evaluate.js report
```

This creates `evaluation-results/evaluation-report.md` with a structured template for documenting results.

### Step 4: Conduct Evaluations

Use GitHub Copilot with the evaluation tools:

1. **For systematic evaluation**: Use the `evaluate-prompts-against-models` prompt
2. **For focused analysis**: Switch to the `prompt-evaluator` chatmode
3. **For specific prompts**: Run individual evaluations and document results

## Evaluation Methodology

### Models Tested

The framework supports evaluation against multiple LLM models:

- **GPT-4.1-mini** - Cost-effective option
- **GPT-4** - High-quality baseline
- **GPT-4 Turbo** - Performance comparison
- **Claude-3.5-Sonnet** - Anthropic alternative
- **Gemini Pro** - Google alternative

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

```markdown
I want to evaluate the `create-implementation-plan.prompt.md` against different models.

Please use the prompt-evaluator chatmode to:
1. Analyze the prompt structure and content
2. Test it against GPT-4.1-mini, GPT-4, and Claude-3.5-Sonnet
3. Compare response quality, speed, and cost
4. Provide optimization recommendations
```

### Example 2: Batch Evaluation

```markdown
Using the evaluate-prompts-against-models prompt, please:
1. Evaluate all C# related prompts and instructions
2. Compare performance across all supported models
3. Identify the most cost-effective model for C# development tasks
4. Generate a comparative report with recommendations
```

### Example 3: Model Selection

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
| GPT-4 | 9.2/10 | 9.4/10 | 9.1/10 | 9.4/10 | 9.0/10 | $0.060 | 3.1s |
| Claude-3.5 | 8.8/10 | 9.2/10 | 8.9/10 | 9.1/10 | 8.7/10 | $0.045 | 2.8s |

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