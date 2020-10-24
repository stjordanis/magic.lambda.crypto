
# Magic Lambda Crypto

Provides cryptographic services to Magic. More specifically, this project provides the following slots.

* __[crypto.hash]__ - Creates a hash of the specified string value/expression's value, using the specified **[algorithm]**, that defaults to SHA256
* __[crypto.password.hash]__ - Creates a cryptographically secure hash from the specified password, expected to be found in its value node. Uses blowfish, or more specifically BCrypt internally, to create the hash with individual salts.
* __[crypto.password.verify]__ - Verifies that a **[hash]** argument matches towards the password specified in its value. The **[hash]** is expected to be in the format created by BCrypt, implying the hash was created with e.g. **[crypto.password.hash]**.
* __[crypto.random]__ - Creates a cryptographically secured random string for you, with the characters [a-zA-Z0-9].
* __[crypto.rsa.create-key]__ - Creates an RSA keypair for you, allowing you to pass in **[strength]**, and/or **[seed]** to override the default strength being 2048, and apply a custom seed to the random number generator. The private/public keypair will be returned to caller as **[public]** and **[private]** after invocation, which is the DER encoded keys, encoded by default as base64.
* __[crypto.rsa.sign]__ - Cryptographically signs a message (provided as value) with the given private **[private-key]**, optionally using the specified hashing **[algorithm]**, defaulting to SHA256, and returns the signature for your content as value. The signature content will be returned as the base64 encoded raw bytes being your signature.
* __[crypto.rsa.verify]__ - Verifies a previously created RSA signature towards its message (provided as value), with the specified public **[public-key]**, optionally allowing the caller to provide a hashing **[algorithm]**, defaulting to SHA256. The slot will throw an exception if the signature is not matching the message passed in for security reasons.
* __[crypto.rsa.encrypt]__ - Encrypts the specified message (provided as value) using the specified public **[public-key]**, and returns the encrypted message as a base64 encoded encrypted message by default.
* __[crypto.rsa.decrypt]__ - Decrypts the specified message (provided as value) using the specified private **[private-key]**, and returns the decrypted message as its original value.
* __[crypto.aes.encrypt]__ - Encrypts a piece of data using the AES encryption algorithm
* __[crypto.aes.decrypt]__ - Decrypts a piece of data previously encrypted using AES encryption
* __[crypto.encrypt]__ - Convenience slot combining AES and RSA encryption to encrypt some message
* __[crypto.decrypt]__ - The opposite of the above
* __[crypto.get-key]__ - Returns the public key that was used to encrypt a message using the above slot. Result is returned in _"fingerprint format"_.

## Supported hashing algorithms

All slots above requiring an **[algorithm]** argument, can use these hashing algorithms by default. Notice, some unsafe hashing
algorithms have been explicitly removed, due to the high risks of creating collisions with them. These includes SHA1 and MD5.

* SHA256
* SHA384
* SHA512

## [crypto.random]

The **[crypto.random]** can optionally take a **[min]** and **[max]** argument, which defines the min/max length of the
string returned. If not supplied, the default values for these arguments are respectively 10 and 20. This slot is useful
for creating random secrets, and similar types of random strings, where you need cryptographically secured random strings.
An example of generating a cryptographically secure random string of text, between 50 and 100 characters in lenght,
can be found below.

```
crypto.random
   min:50
   max:100
```

Notice, the **[crypto.random]** slot will _only_ return characters from a-z, A-Z and 0-9. Which makes
it easily traversed using any string library. However, you can provide a **[raw]** argument, and set its
value to boolean true, at which point the slot will return the raw bytes as a `byte[]`. This has a much
larger amount of entropy than simply using alphanumeric characters, for the same bit size - Which is
important as you start creating keys for AES encryption, etc.

## [crypto.hash]

The **[crypto.hash]** slot can be used to generate hash values. When you invoke it, you can choose
between having the hash returned as raw a `byte[]`, as a _"fingerprint"_ or as its bit encoded hex
version. Below is an example of all three of these formats.

```
.data:Some data to hash
crypto.hash:x:@.data
   format:fingerprint
crypto.hash:x:@.data
   format:text

// Commented out since Magic can't display byte[] as Hyperlambda
.crypto.hash:x:@.data
   format:raw
```

## Cryptography

This library also supports several cryptographic services, but first a bit of cryptography theory.
Public key cryptography, or what's often referred to as _"asymmetric cryptography"_ is based upon
a *key pair*. One of your keys are intended for being publicly shared, and is often referred to
as _"your public key"_. This key can do two important things.

1. It can encrypt data such that *only* its private counterpart key can decrypt the data
2. It can verify that a message originated from a party that has access to its private counterpart

Hence, keeping your *private* key as just that, implying **private**, is of outmost importance, otherwise 3rd
parties might read messages others send to you, and also impersonate you in front of others. In addition, securely
delivering your public key to the other party, is of equal importance, to make sure they're using the *correct*
public key in their communication with you. If you can keep your private key private,
and securely deliver your public key to others, you have a 100% secure channel to use for communication,
preventing malicious individuals from both reading what others send to you, and also tampering with the
content you send to others, before the other party receives it. Hence, cryptography is about two main subjects.

1. Encrypting messages sent *to you*
2. Allowing you to provide guarantees that a message originated *from you*

Depending upon your paranoia level, you might just send your public key in an email, which is considered insecure -
Or you might need to physically meet the person whom you want to communicate with,
and give him a USB stick with your public key, which is considered full paranoia level. The latter might be important
if you fear what's often referred to as a _"man in the middle attack"_, where some malicious agent, takes your public key,
and gives a bogus and fake public key to the other party. This results in that the man in the middle
can intercept your communication, decrypt it, and re-encrypt it with your public key, before he or she
sends it to you - In addition to that he can use a similar mechanism to impersonate your signatures,
allowing the other party to falsely believe some message originated from you, when it did indeed originate
from a malicious _"man in the middle"_.

There are several different ways to create a key pair, just have the above in mind as you start using
cryptography in your Hyperlambda applications. Most of the cryptography functions in this library is
using Bouncy Castle, which is a thoroughly tested library for doing cryptography. Bouncy Castle is
owned by a charitable organisation in Australia, so they don't need to obey by American laws, reducing
American intelligence services ability to lawfully force them to build backdoors and similar constructs
into their code. Bouncy Castle is also Open Source, allowing others to scrutinise their code for such
backdoors. However, with cryptography, there *are no guarantees*, only a _"general feeling and concent"_
amongst developers that it's secure.

### Creating an RSA keypair

To create an RSA keypair that you can use for other cryptographic services later, you can use something as follows.

```
crypto.rsa.create-key
   strength:2048
   seed:some random jibberish text
```

Both the **[strength]** and **[seed]** is optional above. Strength will default to 2048, which might be too little
for serious cryptography, but increasing your strength too much, might result in the function spending several
seconds, possibly minutes to return if you set it too high - In addition to that your key pair becomes very large.
The **[seed]** is optional, and even if you don't provide a seed argument, the default seed should still be strong
enough to avoid predictions.

A good strength for an RSA key, is considered to be 4096, which developers around the world feels are secure enough
to avoid brute force _"guessing"_ of your private key. According to what we know about cryptography, all other concerns
set aside, a 4096 bit strength key pair, *should* be impossible to break. If you're just playing around with cryptography
to learn, 1024 is probably more than enough.

Notice, if you want the key back as raw bytes, you can supply a **[raw]** argument, and set its value to boolean
true, at which point the returned key(s) will only be DER encoded, and returned as a raw `byte[]`. This might be
useful, if you for instance need to persist the key to disc, as a binary file, etc. All the RSA slots can return
their results as `byte[]` values, if you provide a **[raw]** argument to them, and set its value to true.
If you don't provide a raw argument, the returned value will be the base64 encoded DER format of your key pair.

This slot will also return the fingerprint of your public key, which is useful to keep around somewhere,
since it's used in other cryptographic operations to identify keys used in operation, etc.

### Cryptographically signing and verifying the signature of a message

You can use a previously created private RSA key to cryptographically sign some data or message, intended to be passed
over an insecure context, allowing the caller to use your public key to verify the message was in fact created
by the owner of the private key. To sign some arbitrary content using your private key, and also verify the message
was correctly signed with a specific key, you can use something as follows.

```
.data:some piece of text you wish to sign

crypto.rsa.create-key

// Notice, using PRIVATE key
crypto.rsa.sign:x:@.data
   key:x:@crypto.rsa.create-key/*/private

// Uncommenting these lines, will make the verify process throw an exception
// set-value:x:@.data
//    .:Some piece of text you wish to sign - XXXX

// Notice, using PUBLIC key
crypto.rsa.verify:x:@.data
   signature:x:@crypto.rsa.sign
   key:x:@crypto.rsa.create-key/*/public
```

If somebody tampers with the content between the signing process and the verify process, an exception will
be thrown during the verify stage. Something you can verify yourself by uncommenting the above **[set-value]**
invocation. Throwing an exception is a conscious choice, due to the potential security breaches an error
in your code might have, creating a false positive if you erronously invert an **[if]** statement. Even though
this is technically _"using exceptions for control flow"_, it has been a conscious design choice
as the library was created, to avoid false positives during the verification process of a signature.

Notice, if you want the signature back as raw bytes, you can supply a **[raw]** argument, and set its value to boolean
true, at which point the returned signature will be returned as a raw `byte[]`. This might be
useful, if you for instance need to persist the signature to disc, as a binary file, etc.
If you don't provide a raw argument, the returned value will be a base64 encoded byte array.

### Encrypting and decrypting a message

To encrypt a message, you can use something as follows.

```
.data:some piece of text you wish to encrypt

crypto.rsa.create-key

crypto.rsa.encrypt:x:@.data
   key:x:@crypto.rsa.create-key/*/public

crypto.rsa.decrypt:x:@crypto.rsa.encrypt
   key:x:@crypto.rsa.create-key/*/private
```

Notice how the encryption above is using the *public key*, and the decryption is using the *private key*. The encrypt slot
will internally base64 encode the encrypted data for simplicity reasons, allowing you to immediately inspect it as text,
since encryption will result in a byte array, which is often inconvenient to handle.
You can override this by passing in a **[raw]** argument, and set its value to true, at which point a `byte[]` will be
returned.

Also notice how the encrypted message is larger than its original string. This is because of something called _"padding"_
in encryption, only being relevant for messages that are smaller in size than your original text. Padding
implies that no encrypted text resulting of en encryption operation can be significantly smaller in size than the
size of the (public) key used to encrypt the message. This is only relevant for small pieces of data, and have
few implications for larger pieces of text being encrypted. However, if you want to transmit *very large* messages,
you might want to *combine* asymmetric cryptography with symmetric cryptography, which we will illustrate later.

Notice, if you want the message back as raw bytes, you can supply a **[raw]** argument, and set its value to boolean
true as you invoke **[crypto.rsa.encrypt]**, at which point the returned encrypted message will be returned as a
raw `byte[]`. This might be useful, if you for instance need to persist the message to disc, as a binary file, etc.
You can also supply **[raw]** as you invoke **[crypto.rsa.decrypt]** if you know the content in the message is
not a string, but rather an array of `byte[]`. Base64 encoding a byte array normally makes it larger in size,
and also require CPU resources in both ends of the communication, making it sometimes important to have the raw byte
array, instead of its base64 encoded version.

### Symmetric cryptography

RSA is asymmetric cryptography, implying a different key is used for *decrypting* the data, than that which
was used to *encrypt* the data. This project also supports symmetric cryptography, more specifically the AES
encryption algorithm. This algorithm requires the *same key* to decrypt some content that was used to encrypt
the data, and the key must either be 128, 192 or 256 bits long. Below is an example.

```
crypto.aes.encrypt:Howdy, this is cool
   password:Howdy World this is a passphrase that guarantees 256 bits strength
crypto.aes.decrypt:x:-
   password:Howdy World this is a passphrase that guarantees 256 bits strength
```

The length of the key argument you provide, becomes the bit strength of the encryption, ranging from
128 through 192 to 256 bits. However, even though the key you normally use for encrypting and decrypting
when using AES is supposed to be a `byte[]`, this project will automatically convert any passphrase
specified from a string to a SHA256 hash value. This allows you to use *any* passphrase you wish,
while avoiding reducing entropy, making it harder to crack the encrypted message.

Even though AES has low bit strength, it's still considered one of the strongest forms of cryptography
that exists, assuming you use it *correct*. For the record, this library does *not* use the built in
AES library from .Net, which has several security issues, due to the way it handles padding, among other
things - Neither does this library simply convert strings to `byte[]` arrays using `Encoding.UTF.GetBytes`,
which *significantly* reduces entropy, and makes your message easily cracked by a malicious agent with
some resources. Instead Magic uses Bouncy Castle, which does not have these security holes, in addition
to that it creates a SHA256 hash of passphrases used, if you provide a string, keeping as much entropy
as possible. However, if you want to decrypt it using *other* libraries, you'll have to inform the other
party of that the passphrase supplied is actually supposed to be hashed using SHA256 before supplied
as the `key` during decryption.

The library also supports using raw `byte[]` values as its **[password]** value, allowing you
to generate a random array of bytes, either 16, 24 or 32 bytes in size, and use this as your
passphrase directly - At which point the byte array will be used as is, and not hashed in any ways
before encryption/decryption occurs.

Due to AES' blistering speed and strength, it is often wise to combine asymmetric cryptography with
symmetric cryptography, which can be used by generating a random symmetric key/passphrase, then encrypt
this passphrase using asymmetric cryptography, such as for instance RSA, for then to use the passphrase
to encrypt the actual main data the caller wants to transmit. This has several advantages, such as
reducing the size of the data sent, while still providing the benefits from asymmetric cryptography,
such as securely sharing the public key, etc. Of course, sharing a symmetric key without major hassle,
and/or making adversaries also get a hold of it, is practically *very difficult* for obvious reasons,
unless you can asymmetrically encrypt the symmetric key.

If you only need a random array of 32 bytes, to use as your passphrase, in combination with for
instance RSA asymmetric cryptography - You can use the **[crypto.random]** slot as follows.

```
crypto.random
   min:32
   max:32
   raw:true
```

## Combining RSA and AES cryptography

AES and RSA are only really useful when combined. Hence, this project contains the following convenience slots.

* __[crypto.encrypt]__ - Encrypts some message using AES + RSA
* __[crypto.decrypt]__ - Decrypts some message using AES + RSA
* __[crypto.get-key]__ - Returns the fingerprint of the RSA key that was used to encrypt some message using **[crypto.encrypt]**

The **[crypto.encrypt]** slot requires some message/content, a signing key, an encryption key, and your signing
key's fingerprint. This slot will first cryptographically sign your message using the private key. Then it
will use the public key supplied to encrypt the message. Below is an example.

```
// Recipient's key.
crypto.rsa.create-key
   strength:512

// Sender's key.
crypto.rsa.create-key
   strength:512

// Encrypting some message.
crypto.encrypt:Some super secret message
   encryption-key:x:././*/crypto.rsa.create-key/[0,1]/*/public
   signing-key:x:././*/crypto.rsa.create-key/[1,2]/*/private
   signing-key-fingerprint:x:././*/crypto.rsa.create-key/[1,2]/*/fingerprint

// Decrypting the above encrypted message.
crypto.decrypt:x:-
   decryption-key:x:././*/crypto.rsa.create-key/[0,1]/*/private
```

**Notice** - We're using only 512 bit strength in the above example. Make sure you (at least) use
2048, preferably 4096 in real world usage.

To understand what occurs in the above Hyperlambda example, let's walk through it step by step, starting from
the **[crypto.encrypt]** invocation.

1. The message _"Some super secret message"_ is first cryptographically signed using the **[signing-key]**
2. The signed message is then encrypted using a CSRNG generated AES key
3. The AES key from the above is then encrypted using the **[encryption-key]**, that's assumed to be the recipient's public key
4. The signing key's fingerprint is stored inside of the encrypted content, such that when the message is decrypted, the other party can verify that the signature originated from some trusted party
5. The encryption key's fingerprint is stored as bytes, prepended before the encrypted message, which allows the other party to retrieve the correct decryption key, according to what encryption key the caller encrypted the message with

Hence, the *only* thing that is in plain sight in the above encrypted message, is the fingerprint of the public
key that was used to encrypt the message. Only after the message is decrypted, the signature for the message
can be retrieved, together with the fingerprint of the key that was used to sign the message. Hence, what would
normally be a more complete process, is that after the receiver decrypts the message, he should also verify that
the signature originates from some trusted party - Such as illustrated below.

```
// Recipient's key.
crypto.rsa.create-key
   strength:512

// Sender's key.
crypto.rsa.create-key
   strength:512

// Encrypting some message.
crypto.encrypt:Some super secret message
   encryption-key:x:././*/crypto.rsa.create-key/[0,1]/*/public
   signing-key:x:././*/crypto.rsa.create-key/[1,2]/*/private
   signing-key-fingerprint:x:././*/crypto.rsa.create-key/[1,2]/*/fingerprint

// Decrypting the above encrypted message.
crypto.decrypt:x:-
   decryption-key:x:././*/crypto.rsa.create-key/[0,1]/*/private

// Verifying signature of encrypted message.
crypto.rsa.verify:x:-
   signature:x:@crypto.decrypt/*/signature
   public-key:x:././*/crypto.rsa.create-key/[1,2]/*/public
```

**Notice** - We're using only 512 bit strength in the above example. Make sure you (at least) use
2048, preferably 4096 in real world usage.

If the above invocation to **[crypto.rsa.verify]** does not throw an exception, we know for a fact that
the message was cryptographically signed with the private key that matches its **[public-key]** argument.
Normally the fingerprint of the sender's key is asssociated with some sort of _"authorisation object"_
to elevate the rights of the user, only *after* having verified the message originated from a trusted
party.

Hence, from the caller's perspective it's *one* invocation to encrypt and sign a message. From the receiver's
perspective it's *two* steps to both decrypt and verify the integrity of a message. The reasons for this,
is because we do *not know* who signed the message, before the message has been decrypted using the receiver's
private key. This prevents any part of the message, except its bare minimum to be provided over your insecure
channel - Giving spoofers and malicious hackers literally nothing to work with, except the fingerprint
of the public key the message was encrypted with.

Hence, malicious adversaries in the middle of the communication, will not know who the message originated from,
only to what decryption key it was addressed. In addition no adversary will be able to read the encrypted content.

### The encryption format

The encrypted package has the following format. Notice, the encryption and signing is a two step process.
Hence, the steps for the first process is as follows.

1. Signing key's fingerprint in SHA256 `byte[]` format, 32 bytes long
2. The length of the signature as `int`, 4 bytes long
3. The actual signature of the message
4. The content of the message in `byte[]` format

**Notice** - At this point *nothing has been encrypted* yet.

Logically it becomes as follows; SHA256(signing_key) + signature_length + signature + plain_text_content.

Afterwards the result from the above steps is encrypted using AES, with a random generated session key 
that is 32 bytes long. And another package is created, which is the final package, intended for being sent
to the recipient. The final encryption package has a structure as follows.

1. Encryption key's fingerprint in SHA256 `byte[]` format, 32 bytes long
2. The length of the encrypted session key as `int`, 4 bytes long
3. The encrypted session key, encrypted using the recipient's public RSA key
4. The AES encrypted content from the above signing step

Logically it becomes as follows; SHA256(RSA_encryption_key) + length_of_encrypted_AES_key +
encrypted_AES_key + AES_encrypted_content. Everything as raw `byte[]`.

Hence, the other party can retrieve the encryption key used for encrypting the package, using for instance
the **[crypto.get-key]** slot on the package. Then the receiver can use his private RSA key to decrypt
the AES key, and use the decrypted AES key to decrypt the rest of the package - Which will result in getting the
package's plain text content, plus the signature, in addition to the fingerprint of the RSA key used to sign
the package. However, all of these steps are done automatically if you use the **[crypto.decrypt]** slot.
Except signature verification. The reasons why the signature verification is a second step, is because
we'll need to supply a public key to verify the signature, and we don't know which RSA key was used to
sign the message, before we have *decrypted* the message.

The AES key is generated using Bouncy Castle's `SecureRandom` implementation, resulting in a 256 bit
cryptography key. This key again is encrypted using whatever bit strength you selected as you created
your RSA key pair. Hence, the message as a whole, is not stronger than whatever key strength you use
as you supply a **[strength]** argument to the **[crypto.rsa.create-key]**.

The above format results in that the only _"meta information"_ an adversary can possibly pick up, is
the fingerprint of the public RSA key used to encrypt the AES key, in addition to also the bit strength
of this RSA key, since the bit strength of an RSA key will result in differences in the length of the
encrypted AES key. An adversary will not have access to who encrypted/transmitted the package, he will
not know who, if any signed the package - Or any other parts of the message - Assuming he is not able
to somehow crack the AES encryption, and/or somehow retrieve the private RSA key the AES package's
encryption key was encrypted with.

## Cryptography concerns

Even assuming you can 100% perfectly communicate in privacy today, your privacy is only as good as a malicious
agent's ability to brute force prime numbers in the case of RSA, and similar techniques with Elliptic Curve.
This means that even though you create an extremely strong key pair according to today's standard - Due to
Moore's law, some 5-10 years down the road, the NSA and the CIA will probably be able to reproduce your private
key, using nothing but your public key as input. And some 10-20 years later, some kid with a pocket calculator,
will also probably be able to do the same. Since agencies such as the FSB, NSA and the MI6 also happens to
vacum clean the internet, for everything transmitted through your ISP, this implies that 5-10 years from now,
they'll be able to read your communication, and figure out what you were talking about some 5-10 years ago.

Also, as quantum computing becomes practical to implement, today's cryptography based upon _"hard problems"_,
will effectively prove useless towards a serious quantum computer's ability to perform multiple math
operations simultaneously, allowing a malicious agent to reproduce your private key in milliseconds, almost
regardless of its strength. So far, we don't know about such quantum computers, but it is assumed they will
become available in the not too distant future for organisations with very deep pockets.

This implies that privacy is like fruit and vegetables; It rots over time. If you can live with this,
you can eliminate most of its concerns, by making sure you periodically create stronger and stronger keypairs,
with higher and higher bit strength. However, in the case quantum computing should somehow be practical,
even such strategies are futile for traditional cryptography, such as EC and RSA. If these are no concerns
for you, you can still use cryptography to have a _"practical form of privacy"_ in your communication,
but have this in mind as you start using cryptography, since there are no certainties when it comes
to this subject. And of course, even if you had access to 100% perfect privacy in your communication with
others, you still need to trust the ones you're communicating with to not tell others about what you
are communicating to them ...

### Torture based decryption

In addition to the above concerns, any shmuck with a baseball bat could probably _"decrypt"_ your
private communication, by simply coercing and torturing the other party to spill the beans. Inevitably,
at some point, everybody breaks. Although there exist ways to counter this too, by for instance start
lying immediately once the torture begins, and/or pretend to be insane - At which point as the torture
victim breaks, he's lied so much, and acted so crazy, that it becomes impossible for the torturer to
believe anything that his victim says - This is probably the simplest way of _"decryption"_ that exists,
and is easily within the means of any gorilla having enough IQ to open a door.

Hence, there is no true privacy, only shades of privacy. This is true regardless of how strong encryption
you are using. Hence ...

> The only true privacy that exists, is never telling anybody anything!

## Quality gates

- [![Build status](https://travis-ci.com/polterguy/magic.lambda.crypto.svg?master)](https://travis-ci.com/polterguy/magic.lambda.crypto)
- [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=alert_status)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=bugs)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=code_smells)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=coverage)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=duplicated_lines_density)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=ncloc)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=security_rating)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=sqale_index)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
- [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.lambda.crypto&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=polterguy_magic.lambda.crypto)
