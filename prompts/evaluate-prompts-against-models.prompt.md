---
mode: 'agent'
tools: ['changes', 'codebase', 'editFiles', 'problems', 'runCommands']
description: 'Evaluate and compare the effectiveness of prompts, instructions, and chatmodes against different LLM models using GitHub Models API with your GitHub token.'
---

# Evaluate Prompts Against Models

Systematically evaluate and compare the effectiveness of prompts, instructions, and chatmodes from the awesome-copilot repository against different LLM models to assess quality, consistency, and performance.

## Primary Objectives

1. **Model Comparison**: Test prompts against multiple LLM models (GPT-4.1-mini, GPT-4, Claude-3.5-Sonnet, etc.)
2. **Quality Assessment**: Evaluate prompt effectiveness, clarity, and output quality
3. **Performance Analysis**: Measure response time, token usage, and cost efficiency
4. **Consistency Testing**: Verify consistent behavior across different model versions
5. **Improvement Recommendations**: Provide actionable suggestions for prompt optimization

## Evaluation Framework

### 1. Prompt Selection and Analysis

**Target Files:**
- `prompts/*.prompt.md` - Reusable prompt templates
- `instructions/*.instructions.md` - Development guidelines and standards
- `chatmodes/*.chatmode.md` - Custom chat mode configurations

**Selection Criteria:**
- Complexity level (simple, moderate, complex)
- Task type (code generation, review, documentation, analysis)
- Domain specificity (language-specific, framework-specific, general)
- Usage frequency and community popularity

### 2. Model Testing Configuration

**Target Models:**
- GPT-4o-mini (latest cost-effective model)
- GPT-4o (standard reference model)
- Phi-3-mini-128k-instruct (Microsoft small model)
- Phi-3-medium-128k-instruct (Microsoft medium model)
- Meta-Llama-3.1-70B-Instruct (Meta large model)
- Meta-Llama-3.1-405B-Instruct (Meta extra large model)
- Mistral-large (Mistral AI large model)
- Mistral-Nemo (Mistral AI compact model)
- Cohere-command-r (Cohere standard model)
- Cohere-command-r-plus (Cohere enhanced model)

**Test Parameters:**
- Temperature: 0.1 (consistent), 0.7 (creative), 1.0 (diverse)
- Max tokens: 2000, 4000, 8000
- Top-p: 0.9, 0.95, 1.0
- Frequency penalty: 0.0, 0.5, 1.0

### 3. Evaluation Metrics

**Quality Metrics:**
- **Accuracy**: Correctness of generated code/content
- **Relevance**: Alignment with prompt requirements
- **Completeness**: Coverage of all requested aspects
- **Clarity**: Readability and understandability
- **Consistency**: Reproducibility across runs

**Performance Metrics:**
- **Response Time**: Average time to generate response
- **Token Usage**: Input/output token consumption
- **Cost Efficiency**: Cost per useful output
- **Success Rate**: Percentage of successful completions

**Prompt-Specific Metrics:**
- **Instruction Following**: Adherence to specific guidelines
- **Format Compliance**: Correct output structure
- **Error Handling**: Graceful handling of edge cases
- **Context Utilization**: Effective use of provided context

### 4. Test Scenarios

**Standard Test Cases:**
```markdown
1. **Basic Functionality Test**
   - Input: Standard use case for the prompt
   - Expected: Correct output following all guidelines
   - Variations: Different project contexts, languages, frameworks

2. **Edge Case Testing**
   - Input: Unusual or boundary conditions
   - Expected: Graceful handling without errors
   - Variations: Empty input, malformed input, extreme values

3. **Context Sensitivity Test**
   - Input: Same prompt with different project contexts
   - Expected: Appropriate adaptation to context
   - Variations: Different codebases, team preferences, domains

4. **Consistency Test**
   - Input: Same prompt run multiple times
   - Expected: Consistent quality and format
   - Variations: Different timestamps, sessions, model instances
```

**Specialized Test Cases:**
```markdown
1. **Code Generation Prompts**
   - Syntax correctness
   - Best practices adherence
   - Security considerations
   - Performance implications

2. **Documentation Prompts**
   - Completeness of documentation
   - Accuracy of technical details
   - Clarity for target audience
   - Proper formatting and structure

3. **Review/Analysis Prompts**
   - Thoroughness of analysis
   - Identification of issues
   - Quality of recommendations
   - Actionability of feedback
```

## Execution Process

### Phase 1: Preparation
1. **Prompt Inventory**: Catalog all prompts, instructions, and chatmodes
2. **Model Setup**: Configure access to target LLM models
3. **Test Case Generation**: Create comprehensive test scenarios
4. **Baseline Establishment**: Run initial tests with GPT-4 as baseline

### Phase 2: Systematic Testing
1. **Batch Processing**: Run tests across all models systematically
2. **Data Collection**: Gather response times, token usage, quality scores
3. **Error Tracking**: Document failures, timeouts, and issues
4. **Result Storage**: Organize results for analysis

### Phase 3: Analysis and Reporting
1. **Statistical Analysis**: Calculate averages, medians, standard deviations
2. **Comparative Analysis**: Compare models across all metrics
3. **Trend Identification**: Identify patterns in performance
4. **Recommendation Generation**: Create actionable insights

## Output Format

### Evaluation Report Structure

```markdown
# Prompt Evaluation Report

## Executive Summary
- **Best Performing Model**: [Model Name] with [Score]
- **Most Cost-Effective**: [Model Name] at [Cost per Token]
- **Fastest Response**: [Model Name] at [Avg Response Time]
- **Most Consistent**: [Model Name] with [Consistency Score]

## Detailed Results

### Model Performance Comparison
| Model | Accuracy | Relevance | Completeness | Clarity | Consistency | Avg Cost | Avg Time |
|-------|----------|-----------|--------------|---------|-------------|----------|----------|
| GPT-4o-mini | 85% | 90% | 88% | 92% | 85% | $0.015 | 2.3s |
| GPT-4o | 92% | 94% | 91% | 94% | 90% | $0.060 | 3.1s |
| Phi-3-mini-128k-instruct | 82% | 85% | 80% | 88% | 82% | $0.005 | 1.8s |

### Prompt-Specific Analysis
#### [Prompt Name]
- **Best Model**: [Model Name] (Score: [X]%)
- **Strengths**: [List key strengths]
- **Weaknesses**: [List areas for improvement]
- **Recommendations**: [Specific optimization suggestions]

## Recommendations

### Model Selection Guidelines
- **For Cost-Sensitive Projects**: Use GPT-4o-mini or Phi-3-mini-128k-instruct
- **For Maximum Quality**: Use GPT-4o or Meta-Llama-3.1-405B-Instruct
- **For Balanced Performance**: Use Mistral-large or Cohere-command-r-plus

### Prompt Optimization Opportunities
1. **High-Impact Improvements**: [List top 3 prompts to optimize]
2. **Model-Specific Tuning**: [Recommendations for specific models]
3. **General Enhancements**: [Universal improvements across all models]
```

## Implementation Guidelines

### Setup Requirements
- GitHub personal access token with GitHub Models access
- Access to GitHub Models API (models.inference.ai.azure.com)
- Authentication via GitHub token (current user)
- Rate limiting and cost monitoring through GitHub
- Result storage and analysis tools

### Testing Best Practices
- Use consistent test environments
- Implement proper error handling
- Track all relevant metadata
- Validate results manually for accuracy
- Document all assumptions and limitations

### Continuous Monitoring
- Regular re-evaluation of prompts
- Monitoring of model updates and changes
- Performance regression detection
- Cost optimization tracking

## Integration with Existing Tools

### Leverage Existing Assets
- **Prompt Engineer Chatmode**: Use for initial prompt analysis
- **Suggest Prompts**: Identify evaluation candidates
- **Update Scripts**: Integrate evaluation results into documentation

### Automation Opportunities
- **CI/CD Integration**: Automated evaluation on prompt changes
- **Performance Alerts**: Notifications for significant changes
- **Cost Monitoring**: Automated budget tracking and alerts

## Expected Outcomes

1. **Data-Driven Model Selection**: Clear guidelines for choosing the best model for specific use cases
2. **Prompt Optimization**: Actionable recommendations for improving prompt effectiveness
3. **Cost Optimization**: Strategies for reducing evaluation and usage costs
4. **Quality Assurance**: Systematic approach to maintaining prompt quality
5. **Performance Benchmarks**: Baseline metrics for future comparisons

## Next Steps

1. **Pilot Evaluation**: Run initial tests on 5-10 representative prompts
2. **Framework Refinement**: Adjust evaluation criteria based on pilot results
3. **Full-Scale Testing**: Evaluate all prompts in the repository
4. **Documentation Update**: Integrate findings into repository documentation
5. **Community Sharing**: Publish evaluation methodology and results

When executing this evaluation, provide detailed analysis, comparative insights, and actionable recommendations for optimizing the awesome-copilot repository's prompts and instructions across different LLM models.