import logging

from pathlib import Path
from .models import Ruleset, RuleOptions
from .utils import exit_with_error

_ALLOWED_RULE_OPTIONS_PER_TYPE = {
    'DOMAIN': ['pre-matching', 'extended-matching'],
    'DOMAIN-SUFFIX': ['pre-matching', 'extended-matching'],
    'DOMAIN-KEYWORD': ['pre-matching', 'extended-matching'],
    'URL-REGEX': []
}


def save_shadowrocket_rulesets(rulesets: list[Ruleset], output_dir: str):
    output_dir_path = Path(output_dir) / 'shadowrocket'

    try:
        output_dir_path.mkdir(parents=True, exist_ok=True)
    except Exception as e:
        exit_with_error(f'Failed to create Shadowrocket output directory: {e}')

    for ruleset in rulesets:
        output_file_path = output_dir_path / f'{ruleset.name}.list'
        lines = []

        for rule in ruleset.rules:
            sr_type = _map_to_shadowrocket_rule_type(rule.type)

            options = _get_shadowrocket_rule_options(rule.options, sr_type)

            line_parts = [sr_type, rule.value] + options
            lines.append(','.join(line_parts))
        try:
            with open(output_file_path, 'w', encoding='utf-8') as output_file:
                output_file.write('\n'.join(lines) + '\n')
        except Exception as e:
            exit_with_error(f'Failed to write Shadowrocket ruleset "{ruleset.name}": {e}')

    logging.info(f'Shadowrocket rulesets saved to: {output_dir_path}')


def _map_to_shadowrocket_rule_type(rule_type: str) -> str:
    match rule_type:
        case 'full':
            return 'DOMAIN'
        case 'suffix':
            return 'DOMAIN-SUFFIX'
        case 'keyword':
            return 'DOMAIN-KEYWORD'
        case 'regexp':
            return 'URL-REGEX'
        case _:
            exit_with_error(f'Unknown rule type: {rule_type}')


def _get_shadowrocket_rule_options(rule_options: RuleOptions, rule_type: str) -> list[str]:
    sr_options = []

    allowed_options = _ALLOWED_RULE_OPTIONS_PER_TYPE.get(rule_type, [])

    options_dict = rule_options.model_dump(by_alias=True, exclude_none=True)

    for option_name in allowed_options:
        option_value = options_dict.get(option_name)

        if option_name in ('pre-matching', 'extended-matching') and option_value is True:
            sr_options.append(option_name)

    return sr_options
