# FilmLogger 3.11 for Workgroups

![GitHub last commit](https://img.shields.io/github/last-commit/jakelandau/filmloggerdotnet) ![GitHub issues](https://img.shields.io/github/issues/jakelandau/FilmLoggerDotNET) ![GitHub](https://img.shields.io/github/license/JakeLandau/FilmLoggerDotNET)

With the creation of FilmLogger in 2016, and the refactoring of FilmLogger v2 in 2021, we took Python and Tkinter as far as they could go. It turns out not very far.

But with Steve Ballmer's next-generation .NET framework, combined with the bold new frontier of TCP/IP networking, FilmLogger has a new lease on life. Welcome to the third generation of FilmLogger technology.

*This*, is **FilmLogger 3.11 for Workgroups.**

## Description

FilmLogger is an app for Windows, macOS and Linux that lets you track what films you watched, when you watched them, and whether you watched at home or at the theater!

![Screenshot of the FilmLogger 3.11 For Workgroups App](https://imgur.com/c05QaXd.jpg)

When you open FilmLogger, you can start adding films immediately, or open a previously saved archive to add more.

Verify your film using it's IMDb ID code! Films are verified using data services provided by [TheMovieDB.org](https://themoviedb.org).

![The Movie DB logo](https://www.themoviedb.org/assets/2/v4/logos/v2/blue_long_1-8ba2ac31f354005783fab473602c34c3f4fd207150182061e425d366e4f34596.svg)

### **FilmLogger REQUIRES a free TheMovieDB.org Account with your own API Key.**


Once your film has been verified, mark the date and location, and then click Add to Archive. A running count of all films in your archive shows in the corner.

**Remember to save before closing!** FilmLogger will prompt you to save if you try to close the app without saving your newly recorded films. We're friendly like that!

## Data Storage

FilmLogger stores data in an *open* `JSON` format, the `FilmLogger v3 standard`! This standard is user-readable, and follows the example below! If you have trouble opening your saved archive, make sure it looks like this:

```json
[
    {
        "imdb_id":"tt1160419",
        "title":"Dune",
        "theater":true,
        "day":21,
        "month":11,
        "year":2021
    },
    {
        "imdb_id":"tt8097030",
        "title":"Turning Red",
        "theater":false,
        "day":3,
        "month":4,
        "year":2022
    },
    ... (and so on)
]
```

The order you put the movies *doesn't actually matter*; computers are pretty smart and any software compatible with the `FilmLogger v3 Standard` can sort your saved `JSON` archive based on the dates of each movie!

## License

Copyright © 2023 Jake Landau

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, specifically version 3.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License, version 3 for more details.

A copy of the license is attached in the file [`LICENSE.MD`](https://github.com/jakelandau/FilmLoggerDotNET/blob/main/LICENSE.md).

## Dependencies
FilmLogger 3.11 For Workgroups uses C# 11 and .NET 7. It relies upon the following packages:

* Avalonia - v0.10.18 - [MIT License](https://github.com/AvaloniaUI/Avalonia/blob/master/licence.md) - Copyright © .NET Foundation and Contributors
* Avalonia.NameGenerator - v1.6.1 - [MIT License](https://github.com/AvaloniaUI/Avalonia.NameGenerator/blob/main/LICENSE) - Copyright © .NET Foundation and Contributors
* AsyncImageLoader.Avalonia - v2.4.0 - [MIT License](https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia/blob/master/LICENSE) - Copyright © 2021 SKProCH
* MessageBox.Avalonia - v2.1.0 - [MIT License](https://github.com/AvaloniaCommunity/MessageBox.Avalonia/blob/master/LICENSE) - Copyright © 2019 CreateLab
* Json.NET - v13.0.2 - [MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md) - Copyright © 2007 James Newton-King
* TMDbLib - v2.0.0 - [MIT License](https://github.com/LordMike/TMDbLib/blob/master/LICENSE.txt) - Copyright © 2016 Michael Bisbjerg

NOTE: The entirety of FilmLogger is licensed under the terms of the AGPLv3-only. The above MIT Licenses only apply to these packages when obtained from the upstream sources; the downstream versions included in FilmLogger become licensed under the AGPLv3-only through it's viral features.
