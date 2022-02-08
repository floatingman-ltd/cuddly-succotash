---
layout: post
title: DaBoM
tags:
  - dotnet
author: walt
date: 2022-02-14
comments: true
---

Play time with **Byte Order Marker**.

## What we are trying to achieve?

It all started when I opened a CSV file for reading and my bytes count was off but three bytes.  Turns out `EF` `BB` `BF` were the three errant bytes.  Strange, when I open the file with a `TextReader` the first `Read()` returned the forth character.  Obviously, there is magic afoot, and since this isn't a magic let's sort this out.  So, the goal is to account for the **Byte Order Marker** (BOM).  Here's the thing, the **BOM** is only relevant in _unicode_ encoded files, and to make it a little more cryptic it is not required in UTF-8 encodings, it's allowed - but not required.

What are the **BOM** then?  As the name indicates it is a byte inserted at the beginning of the file to indicate to a byte reader the order in which the bytes should be interpreted.  There are three character sizes in _unicode_ 8, 16 and 32 bits or one, two and four bytes, commonly referred to as UTF-8, UTF-16 and UTF-32.  With the two larger encodings it is possible to reverse the order of the bytes and we end up with [big-endian and little-endian](https://en.wikipedia.org/wiki/Endianness), I don't want to have that particular discussion - maybe in a later post.

Encoding | Endianness | BOM
--: | --- | ---
UTF-8 | n/a | EF BB BF
UTF-16 | big | FE FF
UTF-16 | little | FF FE
UTF-32 | big | 00 00 FE FF
UTF-32 | little | FF FE 00 00

> For reasons, there was also a UTF-7, which was effectively ASCII encoding, however it could have a BOM associated with it.  It is not recommended to use this encoding at this point.  I'll include the marker here on the off chance you need it.  Remember, it has been marked as `deprecated` in _dotnet_ and will give a compile time warning.

Encoding | Endianness | BOM
--: | --- | ---
UTF-7 | n/a | 2B 2F 76
## How are we going to do this?

There are several items that expose the **BOM**, but first some background, in case you need more information on the **BOM** in general:

- [Wikipedia](https://simple.wikipedia.org/wiki/Byte_order_mark)
- [W3](https://www.w3.org/International/questions/qa-byte-order-mark)
- [unicode](https://unicode.org/faq/utf_bom.html)

1. First let's find some ways of actually 'seeing' the **BOM** in code.  I suspect there will be some stuff in the `System.Text` namespace so let's start there.  
2. The next place we'll go is some stream readers and writers, to see if we can capture a glimpse of

Okay, so what **BOM**s does _dotnet_ support and where are they hidden?  Well, as I kinda gave this away already let's just go and have a look at what's in [`System.Text`](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding).  Right there at the top, _dotnet_ supports five encodings,  we can drill down a bit further and get that expanded to include _big_ and _little_ endian versions.  We get seven encoding out of the box.  

> As a side note, same as last time, we will just be playing in an xUnit project using _fluent assertions_.


## Enough - Show me some code!

```csharp
[Fact]
public void CanSeeBomOnUTF8()
{
    var bom = new UTF8Encoding(true);

    var preamble = bom.GetPreamble();
    preamble[0].Should().Be(0xEF);
    preamble[1].Should().Be(0xBB);
    preamble[2].Should().Be(0xBF);
}
```

This gives us a passing grade, let's add a string.

