#!/usr/bin/env python3

"""
Create a macOS .app bundle from a published .NET client output.

The bundle uses a small launcher in Contents/MacOS and stores the published
client files in Contents/Resources.
"""

from __future__ import annotations

import argparse
import plistlib
import shutil
from pathlib import Path


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Wrap a published client output in a macOS .app bundle."
    )
    parser.add_argument("--directory", required=True, help="Published client output directory")
    parser.add_argument("--output-dir", required=True, help="Directory to place the .app bundle in")
    parser.add_argument("--name", required=True, help="Visible app name and launcher name")
    parser.add_argument("--apphost", required=True, help="Published apphost filename to launch")
    parser.add_argument("--identifier", required=True, help="Bundle identifier")
    parser.add_argument("--icon", help="Optional .icns file to copy into the bundle")
    return parser.parse_args()


def make_launcher(launcher_path: Path, apphost: str) -> None:
    launcher_path.write_text(
        "#!/bin/sh\n"
        "\n"
        'BASEDIR="$(CDPATH= cd -- "$(dirname "$0")" && pwd)"\n'
        'cd "$BASEDIR"\n'
        f'exec ../Resources/{apphost} "$@"\n',
        encoding="utf-8",
    )
    launcher_path.chmod(0o755)


def copy_publish_output(source_dir: Path, resources_dir: Path) -> None:
    for child in source_dir.iterdir():
        destination = resources_dir / child.name
        if child.is_dir():
            shutil.copytree(child, destination, symlinks=True)
        else:
            shutil.copy2(child, destination, follow_symlinks=True)


def write_info_plist(bundle_dir: Path, name: str, identifier: str, icon: Path | None) -> None:
    plist = {
        "CFBundleName": name,
        "CFBundleDisplayName": name,
        "CFBundleExecutable": name,
        "CFBundleIdentifier": identifier,
        "LSApplicationCategoryType": "public.app-category.games",
    }

    if icon is not None:
        plist["CFBundleIconFile"] = icon.stem

    plist_path = bundle_dir / "Contents" / "Info.plist"
    with plist_path.open("wb") as handle:
        plistlib.dump(plist, handle)


def main() -> None:
    args = parse_args()

    source_dir = Path(args.directory).resolve()
    output_dir = Path(args.output_dir).resolve()
    icon = Path(args.icon).resolve() if args.icon else None

    if not source_dir.is_dir():
        raise SystemExit(f"Publish directory does not exist: {source_dir}")

    if not (source_dir / args.apphost).exists():
        raise SystemExit(f"Published apphost was not found: {source_dir / args.apphost}")

    if icon is not None and not icon.is_file():
        raise SystemExit(f"Icon file does not exist: {icon}")

    bundle_dir = output_dir / f"{args.name}.app"
    macos_dir = bundle_dir / "Contents" / "MacOS"
    resources_dir = bundle_dir / "Contents" / "Resources"

    if bundle_dir.exists():
        shutil.rmtree(bundle_dir)

    macos_dir.mkdir(parents=True, exist_ok=True)
    resources_dir.mkdir(parents=True, exist_ok=True)

    make_launcher(macos_dir / args.name, args.apphost)
    copy_publish_output(source_dir, resources_dir)

    if icon is not None:
        shutil.copy2(icon, resources_dir / icon.name)

    write_info_plist(bundle_dir, args.name, args.identifier, icon)


if __name__ == "__main__":
    main()
