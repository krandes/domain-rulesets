# Domain Rulesets

A collection of domain rulesets for routing, optimized primarily for the Russian web segment.
Rulesets are authored as human-friendly YAML files and compiled into ready-to-use routing formats:

- **geosite** (`geosite.dat`) for [V2Ray](https://www.v2ray.com/) / [Xray](https://github.com/XTLS/Xray-core)
- **Shadowrocket** rule lists (`.list`)

## How it works

Source rulesets live in [`rulesets/`](rulesets) as YAML. A small .NET console app reads them, resolves `includes`,
removes duplicates, and exports the target formats. On every push to `main` that touches the rulesets
or the builder, CI runs the exporter and publishes the compiled artifacts to the **`release`** branch.

## Repository structure

```
rulesets/
  brands/   # per-service rulesets (banks, marketplaces, media, etc.)
  common/   # shared/utility lists (ip-checkers, local)
  geo/      # geographic aggregates and TLD lists (ru, ru-tlds, by-tlds, ...)
src/        # the .NET exporter
```

## Ruleset format

Each YAML file describes a single ruleset:

```yaml
name: example                 # unique ruleset name (used as the geosite tag / .list filename)
rules:
  - type: full                # exact domain match
    value: www.example.com
  - type: suffix              # domain and all subdomains
    value: example.com
  - type: keyword             # substring match
    value: example
  - type: regexp              # regular expression match
    value: ^.*\.example\.(com|org)$
    options:
      attributes:             # optional tags, exported only to geosite (e.g. geosite:example@cn)
        - cn
includes:                     # optional: merge rules from other rulesets by name
  - another-ruleset
```

### Notes

- `name` must be unique across all files; the file name itself does not have to match.
- A ruleset must define at least one `rule` or one `include`.
- `includes` are merged recursively; circular references are rejected at build time.
- `attributes` are only meaningful for geosite and are ignored by the Shadowrocket export.

## Building locally

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project ./src/DomainRulesets.csproj -- \
  -f shadowrocket v2ray \
  -i ./rulesets \
  -o ./release
```

| Option              | Description                              | Default              |
|---------------------|------------------------------------------|----------------------|
| `-f`, `--formats`   | Target formats (space-separated)         | `shadowrocket v2ray` |
| `-i`, `--input-dir` | Directory containing input YAML rulesets | `./rulesets`         |
| `-o`, `--output-dir`| Directory for the exported rulesets      | `./release`          |

Output is written to a per-format subdirectory: `release/v2ray/geosite.dat` and
`release/shadowrocket/<name>.list`.

## Credits

- [v2fly/domain-list-community](https://github.com/v2fly/domain-list-community)