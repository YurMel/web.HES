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
  Installing EPEL Repository and Nginx

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

### 1. Postinstalling and Securing MySQL Server

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

### 2. Installing and Cloning a GitHub Repository

```shell
  $ sudo yum install git && cd /opt
  $ sudo git clone https://github.com/HideezGroup/web.HES && cd web.HES/HES.Web/
```

### 3. Compiling Hideez Enterprise Server

```shell
  $ sudo mkdir /opt/HideezES
  $ sudo dotnet publish -c release -v d -o "/opt/HideezES" --framework netcoreapp2.2 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/web.HES/HES.Web/Crypto_linux.dll /opt/HideezES/Crypto.dll
```
### 4. Creating MySQL User and Database for Hideez Enterprise Server

  Configuring MySQL Server

```shell
  mysql -h localhost -u root -p
```

```sql
  ### CREATE DATABASE
  mysql> CREATE DATABASE hideez;

  ### CREATE USER ACCOUNT
  mysql> CREATE USER 'hideez'@'127.0.0.1' IDENTIFIED BY '<your_secret>';

  ### GRANT PERMISSIONS ON DATABASE
  mysql> GRANT ALL ON hideez.* TO 'hideez'@'127.0.0.1';

  ###  RELOAD PRIVILEGES
  mysql> FLUSH PRIVILEGES;
```

### 5. Configuring Hideez Enterprise Server (MySQL Credentials)

```shell
  $ sudo vi /opt/HideezES/appsettings.json
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

  Daemonizing Hideez Enterprise Server

```shell
  $ sudo cat <<EOF > /lib/systemd/system/hideez.service
  [Unit]
  Description=Hideez Enterprise Service

  [Service]

  User=root
  Group=root

  Environment=BASE_DIR=/opt/HideezES/
  ExecStart=${BASE_DIR}/HES.Web
  Restart=on-failure
  ExecReload=/bin/kill -HUP $MAINPID
  KillMode=process
  # SyslogIdentifier=dotnet-sample-service
  # PrivateTmp=true

  [Install]
  WantedBy=multi-user.target
  EOF

  $ sudo systemctl enable hideez.service
  $ sudo systemctl restart hideez.service
```

### 4. Configuring Nginx Reverse Proxy

  Creating a Self-Signed SSL Certificate for Nginx

```shell
 $ sudo mkdir /etc/nginx/certs
 $ sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/nginx/certs/hideez.key -out /etc/nginx/certs/hideez.crt
```

  Basic Configuration for an Nginx Reverse Proxy

```conf
    server {
        listen       80 default_server;
        listen       [::]:80 default_server;
        server_name  hideez.example.com;

        location / {
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

            # Enable proxy websockets for the Hideez Client to work
            proxy_http_version 1.1;
            proxy_buffering off;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection $http_connection;
            proxy_pass http://localhost:5000;
        }
  ...
```

```conf
    server {
        listen       443 ssl http2 default_server;
        listen       [::]:443 ssl http2 default_server;
        server_name  hideez.example.com;

        ssl_certificate "certs/hideez.crt";
        ssl_certificate_key "certs/hideez.key";

        location / {
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

            # Enable proxy websockets for the hideez Client to work
            proxy_http_version 1.1;
            proxy_buffering off;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection $http_connection;
            proxy_pass https://localhost:5001;
        }
  ...
```

   Restarting Nginx Reverse Proxy and check status

```shell
  $ sudo systemctl restart nginx
  $ sudo systemctl status nginx
  ● nginx.service - The nginx HTTP and reverse proxy server
     Loaded: loaded (/usr/lib/systemd/system/nginx.service; disabled; vendor preset: disabled)
     Active: active (running) since Fri 2019-11-08 20:46:28 EET; 6min ago
    Process: 14756 ExecStart=/usr/sbin/nginx (code=exited, status=0/SUCCESS)
    Process: 14754 ExecStartPre=/usr/sbin/nginx -t (code=exited, status=0/SUCCESS)
    Process: 14752 ExecStartPre=/usr/bin/rm -f /run/nginx.pid (code=exited, status=0/SUCCESS)
   Main PID: 14758 (nginx)
     CGroup: /system.slice/nginx.service
             ├─14758 nginx: master process /usr/sbin/nginx
             └─14760 nginx: worker process
```
### 5. Updating


## Runing into the Docker
  * Install Docker according official [documentation](https://docs.docker.com/install/linux/docker-ce/debian/)