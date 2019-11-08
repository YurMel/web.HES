<p align="center"><img src="https://cdn.shopify.com/s/files/1/0007/8017/3348/files/HideezLogo_Horizontal_360x.jpg" alt="Hideez"></p>

# Hideez Enterprise Server

Hideez Enterprise Server is an HTTP and HTTPS Service that collects and manage log\pass credentials.

## Requirements

  * Git
  * .NET Core (.NET Core SDK version 2.2).
  * MySQL Server (version 8.0+).

## Preparation System (Example for [CentOS 7](https://www.centos.org/about/)).

  Disabling SELinux:

```shell
  $ sudo sed 's/SELINUX=enforcing/SELINUX=disabled/' /etc/sysconfig/selinux
  $ sudo setenforce 0
```
  Install EPEL Repository and Nginx

```shell
  $ sudo yum install epel-release
  $ sudo yum install nginx
  $ sudo systemctl enable nginx
```

  Adding Microsoft Package Repository and Installing .NET Core:

```shell
  $ sudo rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
  $ sudo yum install dotnet-sdk-2.2
```

  Adding MySQL Package Repository and Installing .NET Core:

```shell
  $ sudo rpm -Uvh https://dev.mysql.com/get/mysql80-community-release-el7-3.noarch.rpm
  $ sudo yum install mysql-server
```

  ## Getting Started.

  MySQL Post Installing and Securing MySQL Server

```shell
  $ sudo mysql_secure_installation
```

  It will prompt for few question’s, which recommended to say yes

```shell
  Enter password for user root:

  The existing password for the user account root has expired. Please set a new password.

  New password:
  Re-enter new password:

  Remove anonymous users? (Press y|Y for Yes, any other key for No) : y

  Disallow root login remotely? (Press y|Y for Yes, any other key for No) : y

  Remove test database and access to it? (Press y|Y for Yes, any other key for No) : y

  Reload privilege tables now? (Press y|Y for Yes, any other key for No) : y
```
  * **[Note]** Find default root password using `sudo grep "A temporary password" /var/log/mysqld.log`

  Enabling and running MySQL Service

```shell
  $ sudo systemctl restart mysqld.service
  $ sudo systemctl enable mysqld.service
```

  Installing and Cloning a GitHub Repository

```shell
  $ sudo yum install git && cd /opt
  $ sudo git clone https://github.com/HideezGroup/web.HES && cd web.HES
```

  Compiling Hideez Enterprise Server

```shell
  $ sudo mkdir /opt/HideezES
  $ sudo dotnet publish -c release -v d -o "/opt/HideezES" --framework netcoreapp2.2 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/web.HES/HES.Web/Crypto_linux.dll /opt/HideezES/Crypto.dll && sudo chmod +x /opt/HideezES/Crypto.dll
```
## Configuring system

  Configuring MySQL Server

```shell
  mysql -h localhost -u root -p
```

```mysql
  ### CREATE DATABASE
  mysql> CREATE DATABASE hideez;

  ### CREATE USER ACCOUNT
  mysql> CREATE USER 'hideez'@'127.0.0.1' IDENTIFIED BY '<your_secret>';

  ### GRANT PERMISSIONS ON DATABASE
  mysql> GRANT ALL ON hideez.* TO 'hideez'@'127.0.0.1';

  ###  RELOAD PRIVILEGES
  mysql> FLUSH PRIVILEGES;
```

  Configuring Hideez Enterprise Server (MySQL Credentials)

```shell
  $ sudo vi /opt/HideezWeb/appsettings.json
```

```json
  {
  "ConnectionStrings": {
    "DefaultConnection": "server=127.0.0.1;port=3306;database=hideez;uid=hideez;pwd=<your_secret>"
  },

  "EmailSender": {
    "Host": "smtp.example.com",
    "Port": 123,
    "EnableSSL": true,
    "UserName": "user@example.com",
    "Password": "password"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Information"
    }
  },

  "AllowedHosts": "*"

```

  Demonizing Hideez Enterprise Server

```shell
  $ sudo cat <<EOF > /lib/systemd/system/hideez.service
  [Unit]
  Description=Hideez Enterprise service

  [Service]

  User=root
  Group=root

  Environment=BASE_DIR=/opt/HideezES/
  ExecStart=${BASE_DIR}/HES.Web
  Restart=on-failure
  # SyslogIdentifier=dotnet-sample-service
  # PrivateTmp=true

  [Install]
  WantedBy=multi-user.target
  EOF
  $ systemctl enable hideez.service
```

## Run into the Docker
  * Install docker according official [documentation](https://docs.docker.com/install/linux/docker-ce/debian/)