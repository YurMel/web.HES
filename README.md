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
  * **[Note]** Find default root password using `sudo grep "A temporary password" /var/log/mysqld.log`

## Getting Started.

  Installing and Cloning a GitHub Repository

```shell
  $ sudo yum install git && cd /opt
  $ sudo git clone -b develop https://github.com/HideezGroup/web.HES && cd web.HES
```

  Compiling Hideez Enterprise Server

```shell
  $ sudo mkdir /opt/HideezWeb
  $ sudo dotnet publish -c release -v d -o "/opt/HideezWeb" --framework netcoreapp2.2 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/web.HES/HES.Web/Crypto_linux.dll /opt/HideezWeb/Crypto.dll && sudo chmod +x /opt/HideezWeb/Crypto.dll
```
  Configuring Hideez Enterprise Server

```shell
  $ sudo vi /opt/HideezWeb/appsettings.json
```

  Demonizing Hideez Enterprise Server

```shell
  $ sudo cat <<EOF > /lib/systemd/system/hideez.service
  [Unit]
  Description=Hideez Enterprise service

  [Service]

  User=root
  Group=root

  WorkingDirectory=BASE_DIR=/opt/HideezWeb/
  ExecStart={BASE_DIR}/HES.Web
  Restart=on-failure
  # SyslogIdentifier=dotnet-sample-service
  # PrivateTmp=true

  [Install]
  WantedBy=multi-user.target
  EOF
  $ systemctl enable hideez
```

## Run into the Docker
  * Install docker according official [documentation](https://docs.docker.com/install/linux/docker-ce/debian/)