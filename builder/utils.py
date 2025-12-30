import sys
import logging
import json

from typing import Never
from pathlib import Path
from .models import Ruleset, Rule


def exit_with_error(message: str) -> Never:
    logging.error(message)
    sys.exit(1)


def load_rulesets(input_dir: str) -> list[Ruleset]:
    input_dir_path = Path(input_dir)

    if not input_dir_path.exists():
        exit_with_error(f'Input path does not exist: {input_dir}')

    if not input_dir_path.is_dir():
        exit_with_error(f'Input path is not a directory: {input_dir}')

    raw_rulesets: dict[str, Ruleset] = {}

    for file_path in input_dir_path.glob('*.json'):
        try:
            with open(file_path, 'r', encoding='utf-8') as file:
                data = json.load(file)

                ruleset = Ruleset.from_dict(name=file_path.stem, data=data)
                raw_rulesets[ruleset.name] = ruleset
        except Exception as e:
            exit_with_error(f'Failed to read "{file_path.name}": {e}')

    resolved_rulesets: list[Ruleset] = []

    for name in raw_rulesets:
        resolved_rulesets.append(_resolve_includes(name, raw_rulesets, stack=[]))

    return resolved_rulesets


def _resolve_includes(name: str, rulesets: dict[str, Ruleset], stack: list[str]) -> Ruleset:
    if name in stack:
        chain = ' -> '.join(stack + [name])
        exit_with_error(f'Circular include detected: {chain}')

    if name not in rulesets:
        exit_with_error(f'Ruleset "{name}" not found (referenced in "{' -> '.join(stack)}")')

    current_ruleset = rulesets[name]

    unique_rules: dict[tuple[str, str], Rule] = {}

    for rule in current_ruleset.rules:
        key = (rule.type, rule.value)

        if key not in unique_rules:
            unique_rules[key] = rule

    new_stack = stack + [name]

    for include_name in current_ruleset.includes:
        included_ruleset = _resolve_includes(include_name, rulesets, new_stack)

        for rule in included_ruleset.rules:
            key = (rule.type, rule.value)

            if key not in unique_rules:
                unique_rules[key] = rule

    return Ruleset(
        name=name,
        rules=list(unique_rules.values()))
