#### 3.0.0-alpha001 - 6.9.2017
* A new type named ParseResult will make parsing easier for both F# and C# users.
* * Changed the way the index of grammar objects is stored.

#### 2.0.0 - 3.9.2017
* Overhauled the low-level parser API. It is a simple 5-line type called `Parser`. Its new design prevents misusing the API (like continuing parsing on a completed parser state), and decouples it from any implementation.
* Also, the GOLDParser class is changed too. It is no more a static class, and its design encourages creating a grammar only once.
* Polished the API; changed the names of some types (like `Production.Nonterminal` to `Head`), added prettier C# names and made some types internal.
* And last yet not least... _Everything_ is documented.

#### 2.0.0-beta001 - 18.8.2017
* The project is now one NuGet package named Farkle.
* The library became faster after performance profiling.

#### 1.0.0 - 08.08.2017
* Initial release.

#### 0.0.1-alpha
* Started the project.
