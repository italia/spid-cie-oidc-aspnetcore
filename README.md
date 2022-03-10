# SPID/CIE OIDC Federation SDK for AspNetCore

![Apache license](https://img.shields.io/badge/license-Apache%202-blue.svg)
![aspnetcore-versions](https://img.shields.io/badge/aspnetcore-3.1%20%7C%205.0%20%7C%206.0-brightgreen)
[![GitHub issues](https://img.shields.io/github/issues/italia/spid-cie-oidc-aspnetcore.svg)](https://github.com/italia/spid-cie-oidc-aspnetcore/issues)
[![Get invited](https://slack.developers.italia.it/badge.svg)](https://slack.developers.italia.it/)
[![Join the #spid openid](https://img.shields.io/badge/Slack%20channel-%23spid%20openid-blue.svg)](https://developersitalia.slack.com/archives/C7E85ED1N/)

The SPID/CIE OIDC Federation Relying Party, written in C# for AspNetCore
> ⚠️ __This project is a WiP, the first stable release for production use will be the v1.0.0.__

## Summary

* [Features](#features)
* [Setup](#setup)
* [Docker compose](#docker-compose)
* [Usage](#usage)
* [Contribute](#contribute)
    * [Contribute as end user](#contribute-as-end-user)
    * [Contribute as developer](#contribute-as-developer)
* [Implementations notes](#implementation-notes)
* [License and Authors](#license-and-authors)

## Setup

> TODO: WiP

## Docker compose

> TODO: Not available until v1.0.0 release

## Usage

> TODO: WiP

## Contribute

Your contribution is welcome, no question is useless and no answer is obvious, we need you.

#### Contribute as end user

Please open an issue if you've discoveerd a bug or if you want to ask some features.

#### Contribute as developer

Please open your Pull Requests on the __dev__ branch. 
Please consider the following branches:

 - __main__: where we merge the code before tag a new stable release.
 - __dev__: where we push our code during development.
 - __other-custom-name__: where a new feature/contribution/bugfix will be handled, revisioned and then merged to dev branch.

In this project we adopt [Semver](https://semver.org/lang/it/) and
[Conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) specifications.

## Implementation notes

This project proposes an implementation of the italian OIDC Federation profile with
__automatic_client_registration__ and the adoption of the trust marks as mandatory.

#### General Features

- SPID and CIE OpenID Connect Relying Party
- OIDC Federation 1.0
  - Automatic client registration
  - Trust chain storage and discovery
  - Federation 
    - RP: build trust chains for all the available OPs
- Multitenancy, a single service can configure many RPs
- Bootstrap Italia Design templates


## License and Authors

This software is released under the Apache 2 License by:

- Daniele Giallonardo <danielegiallonardo83@gmail.com>.

