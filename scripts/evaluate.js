#!/usr/bin/env node

/**
 * Prompt Evaluation Orchestrator
 * 
 * This script helps orchestrate the evaluation of prompts, instructions, and chatmodes
 * against different LLM models. It provides utilities for:
 * - Discovering evaluation targets
 * - Organizing evaluation results
 * - Generating reports
 * - Managing evaluation workflows
 */

const fs = require('fs');
const path = require('path');

// Load environment variables from .env file if available
function loadEnvFile() {
  const envPath = path.join(__dirname, '.env');
  if (fs.existsSync(envPath)) {
    const envContent = fs.readFileSync(envPath, 'utf8');
    envContent.split('\n').forEach(line => {
      line = line.trim();
      if (line && !line.startsWith('#') && line.includes('=')) {
        const [key, ...valueParts] = line.split('=');
        const value = valueParts.join('=').trim();
        // Only set if not already set in environment
        if (!process.env[key.trim()]) {
          process.env[key.trim()] = value;
        }
      }
    });
  }
}

// Load .env file before other operations
loadEnvFile();

// Configuration
const CONFIG = {
  // Directories to scan for evaluation targets
  directories: {
    prompts: '../prompts',
    instructions: '../instructions', 
    chatmodes: '../chatmodes'
  },
  
  // File extensions to include
  extensions: {
    prompts: '.prompt.md',
    instructions: '.instructions.md',
    chatmodes: '.chatmode.md'
  },
  
  // Output directory for evaluation results
  outputDir: 'evaluation-results',
  
  // GitHub Models to evaluate against
  models: [
    'GPT-4.1-mini',
    'Phi-4-mini-instruct',
    'Meta-Llama-3.1-8B-Instruct',
    'Mistral-Nemo'
  ],
  
  // Evaluation metrics
  metrics: [
    'accuracy',
    'relevance', 
    'completeness',
    'clarity',
    'consistency',
    'response_time',
    'token_usage',
    'cost_efficiency'
  ]
};

/**
 * Utility function to safely read files
 */
function safeFileOperation(operation, errorMessage = 'File operation failed') {
  try {
    return operation();
  } catch (error) {
    console.error(`${errorMessage}: ${error.message}`);
    return null;
  }
}

/**
 * Extract frontmatter from a file
 */
function extractFrontmatter(filePath) {
  return safeFileOperation(() => {
    const content = fs.readFileSync(filePath, 'utf8');
    const lines = content.split('\n');
    
    if (lines[0] !== '---') return {};
    
    let frontmatter = {};
    let inFrontmatter = false;
    
    for (let i = 1; i < lines.length; i++) {
      const line = lines[i];
      
      if (line === '---') {
        inFrontmatter = false;
        break;
      }
      
      if (i === 1) inFrontmatter = true;
      
      if (inFrontmatter) {
        const match = line.match(/^([^:]+):\s*(.*)$/);
        if (match) {
          const key = match[1].trim();
          let value = match[2].trim();
          
          // Remove quotes if present
          if ((value.startsWith('"') && value.endsWith('"')) || 
              (value.startsWith("'") && value.endsWith("'"))) {
            value = value.slice(1, -1);
          }
          
          frontmatter[key] = value;
        }
      }
    }
    
    return frontmatter;
  }, `Failed to extract frontmatter from ${filePath}`);
}

/**
 * Extract title from a file
 */
function extractTitle(filePath) {
  return safeFileOperation(() => {
    const content = fs.readFileSync(filePath, 'utf8');
    const lines = content.split('\n');
    
    let inFrontmatter = false;
    let frontmatterEnded = false;
    
    for (const line of lines) {
      if (line.trim() === '---') {
        if (!inFrontmatter) {
          inFrontmatter = true;
        } else if (inFrontmatter && !frontmatterEnded) {
          frontmatterEnded = true;
        }
        continue;
      }
      
      if (frontmatterEnded && line.startsWith('# ')) {
        return line.substring(2).trim();
      }
    }
    
    // Fallback to filename
    const basename = path.basename(filePath);
    return basename.split('.')[0].replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
  }, `Failed to extract title from ${filePath}`);
}

/**
 * Discover all evaluation targets
 */
function discoverEvaluationTargets() {
  const targets = {
    prompts: [],
    instructions: [],
    chatmodes: []
  };
  
  for (const [type, directory] of Object.entries(CONFIG.directories)) {
    const fullPath = path.join(process.cwd(), directory);
    
    if (!fs.existsSync(fullPath)) {
      console.warn(`Directory not found: ${fullPath}`);
      continue;
    }
    
    const files = fs.readdirSync(fullPath)
      .filter(file => file.endsWith(CONFIG.extensions[type]))
      .map(file => {
        const filePath = path.join(fullPath, file);
        const frontmatter = extractFrontmatter(filePath);
        const title = extractTitle(filePath);
        
        return {
          filename: file,
          path: filePath,
          title: title,
          description: frontmatter.description || 'No description available',
          type: type,
          frontmatter: frontmatter
        };
      });
    
    targets[type] = files;
  }
  
  return targets;
}

/**
 * Generate evaluation plan
 */
function generateEvaluationPlan(targets) {
  const plan = {
    metadata: {
      generated: new Date().toISOString(),
      totalTargets: 0,
      targetsByType: {}
    },
    evaluationMatrix: [],
    recommendations: []
  };
  
  // Calculate totals
  for (const [type, items] of Object.entries(targets)) {
    plan.metadata.targetsByType[type] = items.length;
    plan.metadata.totalTargets += items.length;
  }
  
  // Generate evaluation matrix
  for (const [type, items] of Object.entries(targets)) {
    for (const item of items) {
      for (const model of CONFIG.models) {
        plan.evaluationMatrix.push({
          target: {
            type: type,
            filename: item.filename,
            title: item.title,
            description: item.description
          },
          model: model,
          metrics: CONFIG.metrics.reduce((acc, metric) => {
            acc[metric] = null; // To be filled during evaluation
            return acc;
          }, {}),
          status: 'pending',
          testCases: [
            'standard_use_case',
            'edge_cases',
            'context_variations',
            'consistency_check'
          ]
        });
      }
    }
  }
  
  return plan;
}

/**
 * Create evaluation report template
 */
function createEvaluationReport(targets) {
  const timestamp = new Date().toISOString().split('T')[0];
  
  let report = `# Awesome Copilot Evaluation Report
Generated: ${new Date().toISOString()}

## Executive Summary

This report presents the evaluation results for prompts, instructions, and chatmodes in the awesome-copilot repository against multiple LLM models.

### Evaluation Scope
- **Total Files Evaluated**: ${targets.prompts.length + targets.instructions.length + targets.chatmodes.length}
- **Prompts**: ${targets.prompts.length}
- **Instructions**: ${targets.instructions.length}
- **Chatmodes**: ${targets.chatmodes.length}
- **Models Tested**: ${CONFIG.models.join(', ')}

### Key Findings
<!-- To be filled after evaluation -->
- **Best Overall Model**: TBD
- **Most Cost-Effective**: TBD
- **Fastest Response**: TBD
- **Most Consistent**: TBD

## Detailed Results

### Model Performance Overview
| Model | Avg Accuracy | Avg Relevance | Avg Completeness | Avg Clarity | Avg Consistency | Avg Cost | Avg Time |
|-------|--------------|---------------|------------------|-------------|-----------------|----------|----------|
`;

  for (const model of CONFIG.models) {
    report += `| ${model} | TBD | TBD | TBD | TBD | TBD | TBD | TBD |\n`;
  }

  report += `

### Evaluation by Type

#### Prompts (${targets.prompts.length} files)
`;

  for (const item of targets.prompts) {
    report += `
##### ${item.title}
- **File**: \`${item.filename}\`
- **Description**: ${item.description}
- **Best Model**: TBD
- **Overall Score**: TBD/10
- **Recommendations**: TBD
`;
  }

  report += `
#### Instructions (${targets.instructions.length} files)
`;

  for (const item of targets.instructions) {
    report += `
##### ${item.title}
- **File**: \`${item.filename}\`
- **Description**: ${item.description}
- **Best Model**: TBD
- **Overall Score**: TBD/10
- **Recommendations**: TBD
`;
  }

  report += `
#### Chatmodes (${targets.chatmodes.length} files)
`;

  for (const item of targets.chatmodes) {
    report += `
##### ${item.title}
- **File**: \`${item.filename}\`
- **Description**: ${item.description}
- **Best Model**: TBD
- **Overall Score**: TBD/10
- **Recommendations**: TBD
`;
  }

  report += `

## Recommendations

### Model Selection Guidelines
<!-- To be filled after evaluation -->
- **For Cost-Sensitive Projects**: TBD
- **For Maximum Quality**: TBD
- **For Balanced Performance**: TBD

### Optimization Opportunities
<!-- To be filled after evaluation -->
1. **High-Impact Improvements**: TBD
2. **Model-Specific Tuning**: TBD
3. **General Enhancements**: TBD

## Methodology

### Evaluation Framework
- **Quality Metrics**: ${CONFIG.metrics.filter(m => !m.includes('_')).join(', ')}
- **Performance Metrics**: ${CONFIG.metrics.filter(m => m.includes('_')).join(', ')}
- **Test Scenarios**: Standard use case, edge cases, context variations, consistency checks

### Test Configuration
- **Models**: ${CONFIG.models.join(', ')}
- **Temperature**: 0.1, 0.7, 1.0
- **Max Tokens**: 2000, 4000, 8000
- **Runs per Test**: 3 (for consistency measurement)

## Next Steps

1. **Implement Recommendations**: Apply high-impact improvements identified
2. **Monitor Performance**: Track changes in evaluation scores over time
3. **Expand Coverage**: Add new models and evaluation scenarios
4. **Automate Process**: Integrate evaluation into CI/CD pipeline

## Appendices

### A. Evaluation Criteria
[Detailed explanation of evaluation metrics and scoring methodology]

### B. Test Cases
[Complete list of test scenarios used for evaluation]

### C. Raw Data
[Link to detailed evaluation data and logs]
`;

  return report;
}

/**
 * Get information about a specific file
 */
function getFileInfo(filePath) {
  return safeFileOperation(() => {
    if (!fs.existsSync(filePath)) {
      console.log(`Error: File not found: ${filePath}`);
      return null;
    }
    
    const frontmatter = extractFrontmatter(filePath);
    const title = extractTitle(filePath);
    const filename = path.basename(filePath);
    
    // Determine file type based on extension
    let fileType = 'unknown';
    if (filename.endsWith('.prompt.md')) {
      fileType = 'prompts';
    } else if (filename.endsWith('.instructions.md')) {
      fileType = 'instructions';
    } else if (filename.endsWith('.chatmode.md')) {
      fileType = 'chatmodes';
    }
    
    return {
      filename,
      path: filePath,
      title,
      description: frontmatter.description || 'No description available',
      type: fileType,
      frontmatter
    };
  }, `Failed to get file info for ${filePath}`);
}

/**
 * Evaluate a specific file against one or all models
 */
function evaluateFile(filePath, model = null) {
  const target = getFileInfo(filePath);
  if (!target) {
    return { error: 'File not found or invalid' };
  }
  
  const modelsToTest = model ? [model] : CONFIG.models;
  const results = {
    file: target,
    evaluations: []
  };
  
  modelsToTest.forEach(testModel => {
    console.log(`Evaluating ${target.filename} with model ${testModel}...`);
    
    // Create a basic test prompt based on the file content
    const fileContent = fs.readFileSync(filePath, 'utf8');
    const testPrompt = `Please evaluate this ${target.type.replace(/s$/, '')} content:\n\n${fileContent}\n\nProvide a brief assessment of its quality and effectiveness.`;
    
    // Note: This is a placeholder for the actual evaluation
    // In a real implementation, you would call the GitHub Models API here
    const evaluation = {
      success: true,
      model: testModel,
      target: target.filename,
      response: 'Evaluation placeholder - API call would be made here',
      response_time: Math.random() * 2,
      note: 'This is a placeholder. Actual GitHub Models API integration would be implemented here.'
    };
    
    results.evaluations.push(evaluation);
  });
  
  return results;
}

/**
 * Main execution function
 */
function main() {
  const command = process.argv[2];
  
  switch (command) {
    case 'discover':
      console.log('Discovering evaluation targets...');
      const targets = discoverEvaluationTargets();
      console.log('Found targets:', JSON.stringify(targets, null, 2));
      break;
      
    case 'plan':
      console.log('Generating evaluation plan...');
      const planTargets = discoverEvaluationTargets();
      const plan = generateEvaluationPlan(planTargets);
      
      // Ensure output directory exists
      if (!fs.existsSync(CONFIG.outputDir)) {
        fs.mkdirSync(CONFIG.outputDir, { recursive: true });
      }
      
      // Write plan to file
      const planFile = path.join(CONFIG.outputDir, 'evaluation-plan.json');
      fs.writeFileSync(planFile, JSON.stringify(plan, null, 2));
      console.log(`Evaluation plan generated: ${planFile}`);
      console.log(`Total evaluations planned: ${plan.evaluationMatrix.length}`);
      break;
      
    case 'report':
      console.log('Creating evaluation report template...');
      const reportTargets = discoverEvaluationTargets();
      const report = createEvaluationReport(reportTargets);
      
      // Ensure output directory exists
      if (!fs.existsSync(CONFIG.outputDir)) {
        fs.mkdirSync(CONFIG.outputDir, { recursive: true });
      }
      
      // Write report to file
      const reportFile = path.join(CONFIG.outputDir, 'evaluation-report.md');
      fs.writeFileSync(reportFile, report);
      console.log(`Evaluation report template created: ${reportFile}`);
      break;
      
    case 'summary':
      console.log('Generating evaluation summary...');
      const summaryTargets = discoverEvaluationTargets();
      console.log('\n=== EVALUATION SUMMARY ===');
      console.log(`Total files: ${summaryTargets.prompts.length + summaryTargets.instructions.length + summaryTargets.chatmodes.length}`);
      console.log(`- Prompts: ${summaryTargets.prompts.length}`);
      console.log(`- Instructions: ${summaryTargets.instructions.length}`);
      console.log(`- Chatmodes: ${summaryTargets.chatmodes.length}`);
      console.log(`Models to test: ${CONFIG.models.length}`);
      console.log(`Total evaluation combinations: ${(summaryTargets.prompts.length + summaryTargets.instructions.length + summaryTargets.chatmodes.length) * CONFIG.models.length}`);
      break;
      
    case 'info':
      if (process.argv.length < 4) {
        console.log('Error: Please provide a file path');
        console.log('Usage: node evaluate.js info <file-path>');
        return;
      }
      
      const filePath = process.argv[3];
      const fileInfo = getFileInfo(filePath);
      if (fileInfo) {
        console.log('\n=== FILE INFORMATION ===');
        console.log(`File: ${fileInfo.filename}`);
        console.log(`Path: ${fileInfo.path}`);
        console.log(`Title: ${fileInfo.title}`);
        console.log(`Description: ${fileInfo.description}`);
        console.log(`Type: ${fileInfo.type}`);
        console.log(`Frontmatter: ${JSON.stringify(fileInfo.frontmatter, null, 2)}`);
      }
      break;
      
    case 'evaluate':
      if (process.argv.length < 4) {
        console.log('Error: Please provide a file path');
        console.log('Usage: node evaluate.js evaluate <file-path> [model]');
        return;
      }
      
      const evaluateFilePath = process.argv[3];
      const evaluateModel = process.argv[4] || null;
      
      if (evaluateModel && !CONFIG.models.includes(evaluateModel)) {
        console.log(`Error: Unknown model '${evaluateModel}'`);
        console.log(`Available models: ${CONFIG.models.join(', ')}`);
        return;
      }
      
      console.log(`Evaluating file: ${evaluateFilePath}`);
      if (evaluateModel) {
        console.log(`Using model: ${evaluateModel}`);
      } else {
        console.log(`Using all models (${CONFIG.models.length} total)`);
      }
      
      const evaluationResult = evaluateFile(evaluateFilePath, evaluateModel);
      
      // Ensure output directory exists
      if (!fs.existsSync(CONFIG.outputDir)) {
        fs.mkdirSync(CONFIG.outputDir, { recursive: true });
      }
      
      // Write results to file
      const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
      const resultFile = path.join(CONFIG.outputDir, `evaluation-result-${timestamp}.json`);
      fs.writeFileSync(resultFile, JSON.stringify(evaluationResult, null, 2));
      
      console.log(`Evaluation completed. Results saved to: ${resultFile}`);
      break;
      
    default:
      console.log('Awesome Copilot Evaluation Orchestrator');
      console.log('');
      console.log('Usage: node evaluate.js <command> [arguments]');
      console.log('');
      console.log('Commands:');
      console.log('  discover              - Discover all evaluation targets');
      console.log('  plan                  - Generate evaluation plan');
      console.log('  report                - Create evaluation report template');
      console.log('  summary               - Show evaluation summary');
      console.log('  info <file-path>      - Show information about a specific file');
      console.log('  evaluate <file-path> [model] - Evaluate a specific file against one or all models');
      console.log('');
      console.log('Arguments:');
      console.log('  <file-path>           - Path to the file to evaluate (e.g., \'../prompts/csharp-async.prompt.md\')');
      console.log('  [model]               - Optional specific model to use (e.g., \'gpt-4o-mini\')');
      console.log('');
      console.log('Examples:');
      console.log('  node evaluate.js discover');
      console.log('  node evaluate.js plan');
      console.log('  node evaluate.js report');
      console.log('  node evaluate.js summary');
      console.log('  node evaluate.js info ../prompts/csharp-async.prompt.md');
      console.log('  node evaluate.js evaluate ../prompts/csharp-async.prompt.md');
      console.log('  node evaluate.js evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini');
      console.log('');
      console.log('Available Models:');
      console.log(`  ${CONFIG.models.join(', ')}`);
      break;
  }
}

// Run the script
if (require.main === module) {
  main();
}

module.exports = {
  discoverEvaluationTargets,
  generateEvaluationPlan,
  createEvaluationReport,
  CONFIG
};