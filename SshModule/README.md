# SshModule

## Overview
The SshModule allows you to establish SSH connections and forward ports using the Plang programming language. This module supports connections via username/password and private key authentication.

## Prerequisites
- Must be using the Plang programming language. See [plang.is](https://plang.is) for more details.
- The private key file must be in RSA PEM format, generated like this:
  ```
  ssh-keygen -t rsa -b 4096 -m PEM -f privatekey.txt
  ```

## Installation
Drop the `SshModule.dll` into your `.modules` folder of your plang project. Once installed, you can start using it in your Plang code as demonstrated below.

## Source Code
The C# source code can be found in the `Program.cs` file and is licensed under the MIT license.

## Usage

### Connect using Username and Password
```plang
Start
/ connect to ssh using username and password, then forward requests going through port
- connect to ssh, 
    host: example.org, port: 22, 
    username: %Settings.SshUsername%, 
    password: %Settings.SshPassword%
- send ssh command 'ls', write to %currentDir%
- write out %currentDir%
```

### Connect using Private Key
```plang
Start
/ connect to ssh using privatekey
- read file 'privatekey.txt', into %sshPrivateKey%
- connect to ssh, 
    name: 'sshWithPrivateKey'
    host: example2.org, port: 22, username: root, 
    private key: %sshPrivateKey%, private key passphrase: %Settings.PrivateKeyPassphrase%
- send ssh command 'ls', write to %currentDir%
- write out %currentDir%
```

### Forward Port to MySQL Instance
```plang
Start
/ lets forward specific port after we connect, this case to MySQL instance, allowing query to db
- connect to ssh, 
    name: 'forwardSsh'
    host: example.org, port: 22, 
    username: %Settings.SshUsername%, 
    password: %Settings.SshPassword%
    forward port: 3306, forward host: 10.0.0.1
- create datasource 'MySqlDb'
- select * from users, write to %users%
- write out %users%
```

## Getting Help
For assistance and more information on using the SshModule, please visit the [Plang Discussion forum](https://github.com/orgs/PLangHQ/discussions) or join the [Discord community](https://discord.gg/A8kYUymsDD).

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.