#!/usr/bin/env python3

"""
Prompt Evaluation Orchestrator - Python Version

This script helps orchestrate the evaluation of prompts, instructions, and chatmodes
against different LLM models using GitHub Models. It provides utilities for:
- Discovering evaluation targets
- Organizing evaluation results
- Generating reports
- Managing evaluation workflows
"""

import json
import os
import sys
import re
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional, Any
import requests

# Load environment variables from .env file if available
def load_env_file():
    """Load environment variables from .env file if it exists."""
    env_path = Path(__file__).parent / '.env'
    if env_path.exists():
        with open(env_path, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#') and '=' in line:
                    key, *value_parts = line.split('=')
                    value = '='.join(value_parts).strip()
                    # Only set if not already set in environment
                    if key.strip() not in os.environ:
                        os.environ[key.strip()] = value

# Load .env file before other operations
load_env_file()

# Configuration
CONFIG = {
    # Directories to scan for evaluation targets
    'directories': {
        'prompts': '../prompts',
        'instructions': '../instructions',
        'chatmodes': '../chatmodes'
    },
    
    # File extensions to include
    'extensions': {
        'prompts': '.prompt.md',
        'instructions': '.instructions.md',
        'chatmodes': '.chatmode.md'
    },
    
    # Output directory for evaluation results
    'output_dir': 'evaluation-results',
    
    # GitHub Models to evaluate against
    'models': [
        'GPT-4.1-mini',
        'Phi-4-mini-instruct',
        'Meta-Llama-3.1-8B-Instruct',
        'Mistral-Nemo'
    ],
    
    # Evaluation metrics
    'metrics': [
        'accuracy',
        'relevance',
        'completeness',
        'clarity',
        'consistency',
        'response_time',
        'token_usage',
        'cost_efficiency'
    ],
    
    # GitHub Models API configuration
    'github_models': {
        'base_url': 'https://models.inference.ai.azure.com',
        'api_version': '2024-05-01-preview'
    }
}

class EvaluationOrchestrator:
    """Orchestrates the evaluation process for prompts, instructions, and chatmodes."""
    
    def __init__(self):
        self.github_token = os.getenv('GITHUB_TOKEN')
        if not self.github_token:
            print("Warning: GITHUB_TOKEN environment variable not set. GitHub Models API calls will fail.")
    
    def safe_file_operation(self, operation, error_message="File operation failed"):
        """Utility function to safely perform file operations."""
        try:
            return operation()
        except Exception as e:
            print(f"{error_message}: {e}")
            return None
    
    def extract_frontmatter(self, file_path: str) -> Dict[str, Any]:
        """Extract frontmatter from a markdown file."""
        def operation():
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            lines = content.split('\n')
            if not lines or lines[0] != '---':
                return {}
            
            frontmatter = {}
            in_frontmatter = False
            
            for i, line in enumerate(lines[1:], 1):
                if line == '---':
                    break
                
                if i == 1:
                    in_frontmatter = True
                
                if in_frontmatter:
                    match = re.match(r'^([^:]+):\s*(.*)$', line)
                    if match:
                        key = match.group(1).strip()
                        value = match.group(2).strip()
                        
                        # Remove quotes if present
                        if (value.startswith('"') and value.endswith('"')) or \
                           (value.startswith("'") and value.endswith("'")):
                            value = value[1:-1]
                        
                        frontmatter[key] = value
            
            return frontmatter
        
        return self.safe_file_operation(operation, f"Failed to extract frontmatter from {file_path}") or {}
    
    def extract_title(self, file_path: str) -> str:
        """Extract title from a markdown file."""
        def operation():
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            lines = content.split('\n')
            in_frontmatter = False
            frontmatter_ended = False
            
            for line in lines:
                if line.strip() == '---':
                    if not in_frontmatter:
                        in_frontmatter = True
                    elif in_frontmatter and not frontmatter_ended:
                        frontmatter_ended = True
                    continue
                
                if frontmatter_ended and line.startswith('# '):
                    return line[2:].strip()
            
            # Fallback to filename
            basename = os.path.basename(file_path)
            return basename.split('.')[0].replace('-', ' ').title()
        
        return self.safe_file_operation(operation, f"Failed to extract title from {file_path}") or "Unknown"
    
    def discover_evaluation_targets(self) -> Dict[str, List[Dict]]:
        """Discover all evaluation targets in the repository."""
        targets = {
            'prompts': [],
            'instructions': [],
            'chatmodes': []
        }
        
        for target_type, directory in CONFIG['directories'].items():
            full_path = Path(directory)
            
            if not full_path.exists():
                print(f"Warning: Directory not found: {full_path}")
                continue
            
            extension = CONFIG['extensions'][target_type]
            files = [f for f in full_path.glob(f'*{extension}')]
            
            for file_path in files:
                frontmatter = self.extract_frontmatter(str(file_path))
                title = self.extract_title(str(file_path))
                
                targets[target_type].append({
                    'filename': file_path.name,
                    'path': str(file_path),
                    'title': title,
                    'description': frontmatter.get('description', 'No description available'),
                    'type': target_type,
                    'frontmatter': frontmatter
                })
        
        return targets
    
    def generate_evaluation_plan(self, targets: Dict[str, List[Dict]]) -> Dict[str, Any]:
        """Generate a comprehensive evaluation plan."""
        plan = {
            'metadata': {
                'generated': datetime.now().isoformat(),
                'total_targets': 0,
                'targets_by_type': {}
            },
            'evaluation_matrix': [],
            'recommendations': []
        }
        
        # Calculate totals
        for target_type, items in targets.items():
            plan['metadata']['targets_by_type'][target_type] = len(items)
            plan['metadata']['total_targets'] += len(items)
        
        # Generate evaluation matrix
        for target_type, items in targets.items():
            for item in items:
                for model in CONFIG['models']:
                    evaluation_entry = {
                        'target': {
                            'type': target_type,
                            'filename': item['filename'],
                            'title': item['title'],
                            'description': item['description']
                        },
                        'model': model,
                        'metrics': {metric: None for metric in CONFIG['metrics']},
                        'status': 'pending',
                        'test_cases': [
                            'standard_use_case',
                            'edge_cases',
                            'context_variations',
                            'consistency_check'
                        ]
                    }
                    plan['evaluation_matrix'].append(evaluation_entry)
        
        return plan
    
    def create_evaluation_report(self, targets: Dict[str, List[Dict]]) -> str:
        """Create a comprehensive evaluation report template."""
        timestamp = datetime.now().strftime('%Y-%m-%d')
        
        total_files = sum(len(items) for items in targets.values())
        
        report = f"""# Awesome Copilot Evaluation Report
Generated: {datetime.now().isoformat()}

## Executive Summary

This report presents the evaluation results for prompts, instructions, and chatmodes in the awesome-copilot repository against multiple LLM models using GitHub Models.

### Evaluation Scope
- **Total Files Evaluated**: {total_files}
- **Prompts**: {len(targets['prompts'])}
- **Instructions**: {len(targets['instructions'])}
- **Chatmodes**: {len(targets['chatmodes'])}
- **Models Tested**: {', '.join(CONFIG['models'])}

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
"""

        for model in CONFIG['models']:
            report += f"| {model} | TBD | TBD | TBD | TBD | TBD | TBD | TBD |\n"

        report += f"""

### Evaluation by Type

#### Prompts ({len(targets['prompts'])} files)
"""

        for item in targets['prompts']:
            report += f"""
##### {item['title']}
- **File**: `{item['filename']}`
- **Description**: {item['description']}
- **Best Model**: TBD
- **Overall Score**: TBD/10
- **Recommendations**: TBD
"""

        report += f"""
#### Instructions ({len(targets['instructions'])} files)
"""

        for item in targets['instructions']:
            report += f"""
##### {item['title']}
- **File**: `{item['filename']}`
- **Description**: {item['description']}
- **Best Model**: TBD
- **Overall Score**: TBD/10
- **Recommendations**: TBD
"""

        report += f"""
#### Chatmodes ({len(targets['chatmodes'])} files)
"""

        for item in targets['chatmodes']:
            report += f"""
##### {item['title']}
- **File**: `{item['filename']}`
- **Description**: {item['description']}
- **Best Model**: TBD
- **Overall Score**: TBD/10
- **Recommendations**: TBD
"""

        report += f"""

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
- **Quality Metrics**: {', '.join([m for m in CONFIG['metrics'] if '_' not in m])}
- **Performance Metrics**: {', '.join([m for m in CONFIG['metrics'] if '_' in m])}
- **Test Scenarios**: Standard use case, edge cases, context variations, consistency checks

### Test Configuration
- **Models**: {', '.join(CONFIG['models'])}
- **GitHub Models API**: {CONFIG['github_models']['base_url']}
- **Authentication**: GitHub Token (current user)
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

### D. GitHub Models Integration
[Details on GitHub Models API usage and authentication]
"""

        return report
    
    def test_github_models_connection(self) -> bool:
        """Test connection to GitHub Models API."""
        if not self.github_token:
            return False
        
        headers = {
            'Authorization': f'Bearer {self.github_token}',
            'Content-Type': 'application/json'
        }
        
        # Test with a simple request to gpt-4o-mini
        url = f"{CONFIG['github_models']['base_url']}/gpt-4o-mini/chat/completions"
        
        data = {
            'messages': [
                {'role': 'user', 'content': 'Hello, this is a test message.'}
            ],
            'max_tokens': 10
        }
        
        try:
            response = requests.post(url, headers=headers, json=data, timeout=10)
            return response.status_code == 200
        except Exception as e:
            print(f"GitHub Models connection test failed: {e}")
            return False
    
    def run_evaluation(self, target_file: str, model: str, test_prompt: str) -> Dict[str, Any]:
        """Run evaluation for a specific target file and model."""
        if not self.github_token:
            return {'error': 'GitHub token not available'}
        
        headers = {
            'Authorization': f'Bearer {self.github_token}',
            'Content-Type': 'application/json'
        }
        
        url = f"{CONFIG['github_models']['base_url']}/{model}/chat/completions"
        
        data = {
            'messages': [
                {'role': 'user', 'content': test_prompt}
            ],
            'max_tokens': 2000,
            'temperature': 0.7
        }
        
        try:
            start_time = datetime.now()
            response = requests.post(url, headers=headers, json=data, timeout=30)
            end_time = datetime.now()
            
            if response.status_code == 200:
                result = response.json()
                return {
                    'success': True,
                    'response': result,
                    'response_time': (end_time - start_time).total_seconds(),
                    'model': model,
                    'target_file': target_file
                }
            else:
                return {
                    'success': False,
                    'error': f'API request failed with status {response.status_code}',
                    'response': response.text
                }
        except Exception as e:
            return {
                'success': False,
                'error': str(e)
            }
    
    def get_file_info(self, file_path: str) -> Optional[Dict[str, Any]]:
        """Get information about a specific file."""
        def operation():
            if not os.path.exists(file_path):
                print(f"Error: File not found: {file_path}")
                return None
            
            frontmatter = self.extract_frontmatter(file_path)
            title = self.extract_title(file_path)
            filename = os.path.basename(file_path)
            
            # Determine file type based on extension
            file_type = "unknown"
            if filename.endswith('.prompt.md'):
                file_type = "prompts"
            elif filename.endswith('.instructions.md'):
                file_type = "instructions"
            elif filename.endswith('.chatmode.md'):
                file_type = "chatmodes"
            
            return {
                'filename': filename,
                'path': file_path,
                'title': title,
                'description': frontmatter.get('description', 'No description available'),
                'type': file_type,
                'frontmatter': frontmatter
            }
        
        return self.safe_file_operation(operation, f"Failed to get file info for {file_path}")
    
    def evaluate_file(self, file_path: str, model: str = None) -> Dict[str, Any]:
        """Evaluate a specific file against one or all models."""
        target = self.get_file_info(file_path)
        if not target:
            return {'error': 'File not found or invalid'}
        
        models_to_test = [model] if model else CONFIG['models']
        results = {
            'file': target,
            'evaluations': []
        }
        
        for test_model in models_to_test:
            print(f"Evaluating {target['filename']} with model {test_model}...")
            
            # Create a basic test prompt based on the file content
            with open(file_path, 'r', encoding='utf-8') as f:
                file_content = f.read()
            
            test_prompt = f"Please evaluate this {target['type'].rstrip('s')} content:\n\n{file_content}\n\nProvide a brief assessment of its quality and effectiveness."
            
            evaluation = self.run_evaluation(file_path, test_model, test_prompt)
            evaluation['model'] = test_model
            evaluation['target'] = target['filename']
            
            results['evaluations'].append(evaluation)
        
        return results

    def main(self):
        """Main execution function."""
        if len(sys.argv) < 2:
            self.print_help()
            return
        
        command = sys.argv[1]
        
        if command == 'discover':
            print('Discovering evaluation targets...')
            targets = self.discover_evaluation_targets()
            print(json.dumps(targets, indent=2))
        
        elif command == 'plan':
            print('Generating evaluation plan...')
            targets = self.discover_evaluation_targets()
            plan = self.generate_evaluation_plan(targets)
            
            # Ensure output directory exists
            output_dir = Path(CONFIG['output_dir'])
            output_dir.mkdir(exist_ok=True)
            
            # Write plan to file
            plan_file = output_dir / 'evaluation-plan.json'
            with open(plan_file, 'w') as f:
                json.dump(plan, f, indent=2)
            
            print(f'Evaluation plan generated: {plan_file}')
            print(f'Total evaluations planned: {len(plan["evaluation_matrix"])}')
        
        elif command == 'report':
            print('Creating evaluation report template...')
            targets = self.discover_evaluation_targets()
            report = self.create_evaluation_report(targets)
            
            # Ensure output directory exists
            output_dir = Path(CONFIG['output_dir'])
            output_dir.mkdir(exist_ok=True)
            
            # Write report to file
            report_file = output_dir / 'evaluation-report.md'
            with open(report_file, 'w') as f:
                f.write(report)
            
            print(f'Evaluation report template created: {report_file}')
        
        elif command == 'summary':
            print('Generating evaluation summary...')
            targets = self.discover_evaluation_targets()
            print('\n=== EVALUATION SUMMARY ===')
            total_files = sum(len(items) for items in targets.values())
            print(f'Total files: {total_files}')
            print(f'- Prompts: {len(targets["prompts"])}')
            print(f'- Instructions: {len(targets["instructions"])}')
            print(f'- Chatmodes: {len(targets["chatmodes"])}')
            print(f'Models to test: {len(CONFIG["models"])}')
            print(f'Total evaluation combinations: {total_files * len(CONFIG["models"])}')
        
        elif command == 'test-connection':
            print('Testing GitHub Models connection...')
            if self.test_github_models_connection():
                print('✓ GitHub Models connection successful')
            else:
                print('✗ GitHub Models connection failed')
                print('Please check your GITHUB_TOKEN environment variable')
        
        elif command == 'info':
            if len(sys.argv) < 3:
                print('Error: Please provide a file path')
                print('Usage: python evaluate.py info <file-path>')
                return
            
            file_path = sys.argv[2]
            file_info = self.get_file_info(file_path)
            if file_info:
                print('\n=== FILE INFORMATION ===')
                print(f"File: {file_info['filename']}")
                print(f"Path: {file_info['path']}")
                print(f"Title: {file_info['title']}")
                print(f"Description: {file_info['description']}")
                print(f"Type: {file_info['type']}")
                print(f"Frontmatter: {json.dumps(file_info['frontmatter'], indent=2)}")
        
        elif command == 'evaluate':
            if len(sys.argv) < 3:
                print('Error: Please provide a file path')
                print('Usage: python evaluate.py evaluate <file-path> [model]')
                return
            
            file_path = sys.argv[2]
            model = sys.argv[3] if len(sys.argv) > 3 else None
            
            if model and model not in CONFIG['models']:
                print(f"Error: Unknown model '{model}'")
                print(f"Available models: {', '.join(CONFIG['models'])}")
                return
            
            print(f"Evaluating file: {file_path}")
            if model:
                print(f"Using model: {model}")
            else:
                print(f"Using all models ({len(CONFIG['models'])} total)")
            
            evaluation_result = self.evaluate_file(file_path, model)
            
            # Ensure output directory exists
            output_dir = Path(CONFIG['output_dir'])
            output_dir.mkdir(exist_ok=True)
            
            # Write results to file
            timestamp = datetime.now().strftime('%Y-%m-%d-%H-%M-%S')
            result_file = output_dir / f'evaluation-result-{timestamp}.json'
            with open(result_file, 'w') as f:
                json.dump(evaluation_result, f, indent=2)
            
            print(f"Evaluation completed. Results saved to: {result_file}")
        
        else:
            self.print_help()
    
    def print_help(self):
        """Print help information."""
        print('Awesome Copilot Evaluation Orchestrator - Python Version')
        print('')
        print('Usage: python evaluate.py <command> [arguments]')
        print('')
        print('Commands:')
        print('  discover              - Discover all evaluation targets')
        print('  plan                  - Generate evaluation plan')
        print('  report                - Create evaluation report template')
        print('  summary               - Show evaluation summary')
        print('  info <file-path>      - Show information about a specific file')
        print('  evaluate <file-path> [model] - Evaluate a specific file against one or all models')
        print('  test-connection       - Test GitHub Models API connection')
        print('')
        print('Arguments:')
        print('  <file-path>           - Path to the file to evaluate (e.g., \'../prompts/csharp-async.prompt.md\')')
        print('  [model]               - Optional specific model to use (e.g., \'gpt-4o-mini\')')
        print('')
        print('Environment Variables:')
        print('  GITHUB_TOKEN          - GitHub personal access token for GitHub Models API')
        print('')
        print('Examples:')
        print('  python evaluate.py discover')
        print('  python evaluate.py plan')
        print('  python evaluate.py report')
        print('  python evaluate.py summary')
        print('  python evaluate.py info ../prompts/csharp-async.prompt.md')
        print('  python evaluate.py evaluate ../prompts/csharp-async.prompt.md')
        print('  python evaluate.py evaluate ../prompts/csharp-async.prompt.md gpt-4o-mini')
        print('  python evaluate.py test-connection')
        print('')
        print('Available Models:')
        print(f'  {", ".join(CONFIG["models"])}')


if __name__ == '__main__':
    orchestrator = EvaluationOrchestrator()
    orchestrator.main()