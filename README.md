License Header Enforcement for OSS Projects
========

[![GitHub](https://img.shields.io/github/license/NightOwl888/SPDX.CodeAnalysis)](https://github.com/NightOwl888/SPDX.CodeAnalysis/blob/main/LICENSE.txt)
[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)

This package provides Roslyn code analyzers to enforce specifying headers in .cs files similar to:

```c#
// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith
// SPDX-License-Identifier: Apache-2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
```

or:

```c#
// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith
// SPDX-License-Identifier: MIT
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

This analyzer does basic checks to ensure that your .cs files:

1. Contain the `SPDX-FileCopyrightText` tag
2. Contain the `SPDX-License-Identifier` tag
3. The `SPDX-License-Identifier` matches sample text from the license

This is just to generate warnings during compile time for ease of maintenance. For more robust validation use [REUSE](https://reuse.software/) during CI.

------------------

## NuGet

[![Nuget](https://img.shields.io/nuget/dt/SPDX.CodeAnalysis)](https://www.nuget.org/packages/SPDX.CodeAnalysis)

------------------

## Saying Thanks

If you find these libraries to be useful, please star us [on GitHub](https://github.com/NightOwl888/NetFx.Polyfills) and consider a sponsorship so we can continue bringing you great free tools like these. It really would make a big difference!

[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)