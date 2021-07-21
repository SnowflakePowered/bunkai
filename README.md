# 分解 bunkai

bunkai is a parser for catalogued ROM file names that follow the following common naming conventions.

* [No-Intro](https://datomatic.no-intro.org/stuff/The%20Official%20No-Intro%20Convention%20(20071030).pdf)
* [TOSEC](https://www.tosecdev.org/tosec-naming-convention)
* [GoodTools](https://raw.githubusercontent.com/SnowflakePowered/shiratsu/25f2c858dc3a9373e27de3df559cd00931d8e55f/shiratsu-naming/src/naming/goodtools/GoodCodes.txt)

bunkai does not use regular expressions and supports a variety of edge cases for each supported naming convention. It is ported from the well tested [shiratsu-naming](https://crates.io/crates/shiratsu-naming) Rust library, but provides a more ergonomic unified C# API. Unlike `shiratsu-naming`, bunkai does not keep trivia such as malformed TOSEC tag orders and is a lossy parser mainly for scraping purposes. 

bunkai is built with the [Pidgin](https://github.com/benjamin-hodgson/Pidgin) parser combinators library.