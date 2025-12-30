from typing import Literal
from pydantic import BaseModel, ConfigDict, Field, model_validator


class RuleOptions(BaseModel):
    model_config = ConfigDict(extra='forbid')

    pre_matching: bool | None = Field(default=None, alias='pre-matching')
    extended_matching: bool | None = Field(default=None, alias='extended-matching')
    attributes: list[str] | None = None


class Rule(BaseModel):
    model_config = ConfigDict(extra='ignore')

    type: Literal['full', 'suffix', 'keyword', 'regexp']
    value: str
    options: RuleOptions = Field(default_factory=RuleOptions)


class Ruleset(BaseModel):
    model_config = ConfigDict(extra='ignore')

    name: str
    rules: list[Rule] = Field(default_factory=list)
    includes: list[str] = Field(default_factory=list)

    @model_validator(mode='after')
    def check_non_empty_rules_or_includes(self) -> 'Ruleset':
        if not self.rules and not self.includes:
            raise ValueError('Ruleset must have at least one rule or one include')

        return self

    @classmethod
    def from_dict(cls, name: str, data: dict):
        return cls.model_validate({'name': name, **data})
