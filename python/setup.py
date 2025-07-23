from setuptools import setup, find_packages
import os

# Read the main README from the repository root
readme_path = os.path.join(os.path.dirname(__file__), '..', 'README.md')
with open(readme_path, "r", encoding="utf-8") as fh:
    long_description = fh.read()

setup(
    name="grid9",
    version="1.0.0",
    author="Grid9 Team",
    description="Precision coordinate compression with uniform 3-meter accuracy globally",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/yourusername/grid9",
    package_dir={'grid9': 'src'},
    packages=['grid9'],
    classifiers=[
        "Development Status :: 5 - Production/Stable",
        "Intended Audience :: Developers",
        "License :: OSI Approved :: MIT License",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.7",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "Programming Language :: Python :: 3.11",
        "Topic :: Software Development :: Libraries :: Python Modules",
        "Topic :: Scientific/Engineering :: GIS",
    ],
    python_requires=">=3.7",
    install_requires=[],
    extras_require={
        "dev": ["pytest", "pytest-cov", "black", "flake8"],
    },
)