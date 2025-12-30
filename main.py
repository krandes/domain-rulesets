import argparse
import logging

from builder.shadowrocket import save_shadowrocket_rulesets
from builder.utils import load_rulesets, exit_with_error
from builder.v2ray import save_v2ray_rulesets

TARGET_FORMATS = ('v2ray', 'shadowrocket')


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description='Domain routing rulesets builder for V2Ray and Shadowrocket',
        formatter_class=argparse.ArgumentDefaultsHelpFormatter
    )

    parser.add_argument(
        '-f', '--formats',
        dest='formats',
        type=str,
        default=','.join(TARGET_FORMATS),
        metavar='<list>',
        help='target formats separated by commas'
    )

    parser.add_argument(
        '-i', '--input-dir',
        dest='input_dir',
        type=str,
        default='./rulesets',
        metavar='<path>',
        help='input directory with JSON rulesets'
    )

    parser.add_argument(
        '-o', '--output-dir',
        dest='output_dir',
        type=str,
        default='./release',
        metavar='<path>',
        help='output directory for generated rulesets'
    )

    args = parser.parse_args()

    return args


def main():
    logging.basicConfig(
        level=logging.INFO,
        format='[%(asctime)s] [%(levelname)s] %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )

    args = parse_args()

    logging.info('Building domain rulesets...')

    formats = [f.strip().lower() for f in args.formats.split(',')]

    if not any(f in TARGET_FORMATS for f in formats):
        exit_with_error(f'No valid formats specified. Available formats: {', '.join(TARGET_FORMATS)}')

    rulesets = load_rulesets(args.input_dir)

    if not rulesets:
        exit_with_error('No rulesets found')

    logging.info(f'Found {len(rulesets)} rulesets')

    if 'shadowrocket' in formats:
        save_shadowrocket_rulesets(rulesets, args.output_dir)

    if 'v2ray' in formats:
        save_v2ray_rulesets(rulesets, args.output_dir)

    logging.info('Build finished successfully!')


if __name__ == '__main__':
    main()
