import setuptools

with open("README.md", "r") as fh:
    long_description = fh.read()

setuptools.setup(
    name="dataapi-pythonclient",
    version="1.7.0",
    author="Jan Scholtyssek",
    author_email="mindleaving@gmail.com",
    description="Python client for communicating with DataAPI",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/mindleaving/dataapi",
    packages=setuptools.find_packages(),
    classifiers=[
        "Programming Language :: Python :: 3",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
    ],
)