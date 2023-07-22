import re
from pathlib import Path
import logging

from attrs import define
from attrs import field

HOME_DIR = Path(__file__).parent.parent
PROJECT_DIR = HOME_DIR / "Chess-Challenge"  # TODO: is there a better way?
DEFAULT_SETTINGS_PATH = PROJECT_DIR / "src/Framework/Application/Core/Settings.cs"


logger = logging.getLogger(__name__)


@define
class Version:
    """
    Version defined as https://semver.org/
    """

    major: int = field(converter=int)
    minor: int = field(converter=int)
    patch: int = field(converter=int)

    VERSION_REGEX = re.compile(
        r"^(?P<major>0|[1-9]\d*)\.(?P<minor>0|[1-9]\d*)\.(?P<patch>0|[1-9]\d*)(?:-(?P<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?P<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$"
    )
    VERSION_IN_FILE_REGEX = re.compile(
        r"^.*version\s*=\s*['\"](?P<version>\d+(\.\d+)*)['\"]\s*;\s*$", re.I
    )

    def __str__(self) -> str:
        return f"{self.major}.{self.minor}.{self.patch}"

    @classmethod
    def from_string(cls, value: str) -> "Version":
        """
        >>> Version.from_string("1.11")
        Traceback (most recent call last):
        ...
        ValueError: Invalid SemVer version: 1.11
        >>> version = Version.from_string("1.2.3")
        >>> str(version)
        '1.2.3'
        >>> version
        Version(major=1, minor=2, patch=3)
        """

        match = cls.VERSION_REGEX.fullmatch(value)
        if not match:
            raise ValueError(f"Invalid SemVer version: {value}")
        values = match.groupdict()
        return cls(
            major=values["major"],
            minor=values["minor"],
            patch=values["patch"],
        )

    @classmethod
    def from_source_file(cls, path: Path) -> "Version":
        with open(path) as fp:
            for line in fp.readlines():
                match = cls.VERSION_IN_FILE_REGEX.fullmatch(line)
                if not match:
                    continue
                str_version = match.groupdict()["version"]
                return cls.from_string(str_version)

        raise ValueError(f"Missing version in file {path}")


def set_output(name: str, value: str) -> None:
    """
    Wrapper function to save a variable in the corresponding GitHub Actions step output.
    """

    import os
    import uuid

    logger.info(f"Setting {name!r} = {value!r}")

    key = "GITHUB_OUTPUT"
    if key not in os.environ:
        logger.warning(
            f"{key} is not found in environment variables, skipping write to it"
        )
        return

    with open(os.environ[key], "a") as fh:
        delimiter = uuid.uuid1()
        print(f"{name}<<{delimiter}", file=fh)
        print(value, file=fh)
        print(delimiter, file=fh)


def main(source_file: Path):
    version = Version.from_source_file(path=source_file)
    logger.info(f"Version found: {version}")
    set_output("version", str(version))


if __name__ == "__main__":
    logging.basicConfig(level=logging.DEBUG)

    main(source_file=DEFAULT_SETTINGS_PATH)
