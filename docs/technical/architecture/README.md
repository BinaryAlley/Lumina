# Architecture Diagrams

This folder contains the C4 architecture diagrams of Lumina. Every diagram is maintained as a PlantUML source file (`.puml`) and rendered to a committed SVG (`.svg`) with the same base name.

## Layout

Every diagram starts with:

```plantuml
!pragma layout elk
```

This selects the Eclipse Layout Kernel (ELK) instead of Graphviz. ELK is bundled with PlantUML since version 1.2024.6, so neither Graphviz nor a separate ELK installation is required. It draws connectors as straight orthogonal segments instead of Bezier curves.

## Rendering

Prerequisites:
- Java 17 or newer.
- PlantUML 1.2026.7, the version recorded in the metadata of the committed SVG files.

Download the pinned PlantUML jar once.

PowerShell:

```powershell
Invoke-WebRequest -Uri "https://repo1.maven.org/maven2/net/sourceforge/plantuml/plantuml/1.2026.7/plantuml-1.2026.7.jar" -OutFile "$env:TEMP\plantuml-1.2026.7.jar"
```

Bash:

```bash
curl -L -o /tmp/plantuml-1.2026.7.jar "https://repo1.maven.org/maven2/net/sourceforge/plantuml/plantuml/1.2026.7/plantuml-1.2026.7.jar"
```

From this folder, render every `.puml` file to an `.svg` file next to it.

PowerShell:

```powershell
cd docs/technical/architecture
java -jar "$env:TEMP\plantuml-1.2026.7.jar" --charset UTF-8 --format svg --ignore-startuml-filename .
```

Bash:

```bash
cd docs/technical/architecture
java -jar /tmp/plantuml-1.2026.7.jar --charset UTF-8 --format svg --ignore-startuml-filename .
```

The options are required for reproducible output:
- `--charset UTF-8` reads the source as UTF-8.
- `--format svg` writes the vector SVG files that are committed.
- `--ignore-startuml-filename` names each output file after its `.puml` source. Without it, PlantUML uses the `@startuml` title as the file name.
- The input `.` renders every diagram in the current folder and writes each SVG beside its source.

Do not add `--output-dir` when rendering this folder. PlantUML resolves a relative output directory against each source file, so `--output-dir .` creates a nested copy of the folder. Running from the folder without that option writes the SVG files in place.

Commit the regenerated `.svg` files together with the `.puml` change. Never edit an SVG by hand.

## Verifying the output

The `.puml` file is the source of truth. The committed SVG carries the PlantUML version marker and rendering hints, but no readable `@startuml` source and no `<metadata>` block. After rendering, check that:
- the SVG contains `<?plantuml 1.2026.7?>`,
- its base name matches its `.puml` source,
- the diagram connectors are straight orthogonal segments, not curved paths.

The only reliable way to tell whether a committed SVG is current is to start from a clean working tree, re-render it from its `.puml` with the pinned command, and then check the generated SVGs for changes. From the architecture folder where the render command runs:

```bash
git diff --exit-code -- '*.svg'
```

If you run the check from the repository root instead, use the full path:

```bash
git diff --exit-code -- 'docs/technical/architecture/*.svg'
```

A non-zero exit code means the render changed one or more SVG files, so the committed ones were stale. Review the diff, commit the regenerated SVGs together with the `.puml` change, and re-run the check.

If a future PlantUML release is adopted, update the version in this document and in the download commands, then re-render every diagram in the same change. Mixing PlantUML versions produces noisy SVG diffs and different layouts.
