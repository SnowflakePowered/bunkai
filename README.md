# 分解 bunkai

bunkai is a parser for catalogued ROM file names that follow the following common naming conventions.

* [No-Intro](https://datomatic.no-intro.org/stuff/The%20Official%20No-Intro%20Convention%20(20071030).pdf)
* [TOSEC](https://www.tosecdev.org/tosec-naming-convention)
* [GoodTools](https://raw.githubusercontent.com/SnowflakePowered/shiratsu/25f2c858dc3a9373e27de3df559cd00931d8e55f/shiratsu-naming/src/naming/goodtools/GoodCodes.txt)

bunkai does not use regular expressions and supports a variety of edge cases for each supported naming convention. It is ported from the well tested [shiratsu-naming](https://crates.io/crates/shiratsu-naming) Rust library, but provides a more ergonomic unified C# API. Unlike `shiratsu-naming`, bunkai does not keep trivia such as malformed TOSEC tag orders and is a lossy parser mainly for scraping purposes. 

bunkai is built with the [Pidgin](https://github.com/benjamin-hodgson/Pidgin) parser combinators library.

## Features
Bunkai is a work in progress and while not all features available in shiratsu-naming will be implemented (particularly any trivia items like TOSEC warnings), the following features are intended.

- [x] No-Intro
  - [x] Scene Number
  - [x] BIOS
  - [x] Title
  - [x] Region
  - [x] Language
    - [x] Multi Language 
  - [x] Version
    - [x] `Rev` Versions
    - [x] `v` Single Prefixed Versions
    - [x] `Version` Single Prefixed Versions with `Alt`
    - [x] Unprefixed `1.x` versions 
    - [ ] `Version` prefixed date versions (Redump BIOS versions)
    - [x] Comma-separated version tags
  - [x] Release
  - [x] Bad Dump 
  - [x] Redump Disc tag
  - [x] Redump Multi-tap tag
- [ ] TOSEC
  - [x] ZZZ-UNK- (Omitted)
  - [x] Demo
  - [x] Dates
    - [x] Undelimited Dates
  - [x] Version
    - [x] `Rev` versions
    - [x] `v` versions
    - [x] Version in flag
  - [x] Title
    - [x] Degenerate titles missing demo or date
    - [x] Unexpected spaces
  - [ ] Publisher
    - [x] by-publisher in Title if ZZZ-UNK
    - [x] by-publisher after tags if ZZZ-UNK    
  - [x] Region
    - [ ] GoodTools region
  - [x] Language
    - [x] Multilanguage
  - [x] System
  - [x] Video
  - [x] Copyright
  - [ ] Media
  - [x] Devstatus
  - [ ] Dump info
  - [ ] More info
- [ ] GoodTools
  - [ ] Region
  - [ ] Year
  - [ ] Translation
  - [ ] `REV` Revision
  - [ ] Version
    - [ ] `VWIPX`
    - [ ] `VFinal_`
    - [ ] `Vunknown`
    - [ ] `V x.xx`
    - [ ] `V bX`
    - [ ] `V nn`
    - [ ] `V nnnn`
    - [ ] `V_`
  - [ ] Language
    - [ ] Multilanguage
  - [ ] Volume
  - [ ] Dump tags
  - [ ] Hack tag
  - [ ] Other
    - [ ] `PD`
    - [ ] `NTSC`
    - [ ] `PAL`
    - [ ] `NTSC-PAL`
    - [ ] `PAL-NTSC`