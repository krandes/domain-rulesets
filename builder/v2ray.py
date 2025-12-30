import logging

from pathlib import Path
from .models import Ruleset
from .utils import exit_with_error
from .pb import v2ray_pb2


def save_v2ray_rulesets(rulesets: list[Ruleset], output_dir: str):
    output_dir_path = Path(output_dir) / 'v2ray'
    output_dir_path.mkdir(parents=True, exist_ok=True)

    output_file_path = output_dir_path / 'geosite.dat'

    geo_site_list = v2ray_pb2.GeoSiteList()

    for ruleset in rulesets:
        entry = geo_site_list.entry.add()
        tag = ruleset.name

        entry.country_code = tag

        for rule in ruleset.rules:
            domain = entry.domain.add()
            domain.value = rule.value
            domain.type = _map_to_v2ray_rule_type(rule.type)

            if rule.options.attributes:
                for attribute in rule.options.attributes:
                    attr = domain.attribute.add()
                    attr.key = attribute.lstrip('@')
                    attr.bool_value = True

    try:
        with open(output_file_path, 'wb') as output_file:
            output_file.write(geo_site_list.SerializeToString())
    except Exception as e:
        exit_with_error(f'Failed to write V2Ray geosite.dat: {e}')

    logging.info(f'V2Ray binary compiled to: {output_file_path}')


def _map_to_v2ray_rule_type(rule_type: str):
    match rule_type:
        case 'full':
            return v2ray_pb2.Domain.Full
        case 'suffix':
            return v2ray_pb2.Domain.RootDomain
        case 'keyword':
            return v2ray_pb2.Domain.Plain
        case 'regexp':
            return v2ray_pb2.Domain.Regex
        case _:
            exit_with_error(f'Unknown rule type: {rule_type}')
