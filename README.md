# CsrpServer

CsrpServerプロジェクトは、Intel FPGAとCypress EZ-USB FX2LPを使ったCSRP(Cheap Software Radio Peripheral)のWindows版サーバーとLinux版サーバーのソースコードです。

![構成図](http://rapidack.sakura.ne.jp/ttl/wp-content/uploads/2019/05/CsrpServer2.png)

# Windows版サーバー

WinFormsアプリケーションです。サーバーがEZ-USB FX2LPのRAMにファームウェアを書き込むのでEEPROMのジャンパーは外しておきます。

![Windows版サーバー](http://rapidack.sakura.ne.jp/ttl/wp-content/uploads/2019/05/CsrpServer.png)

## Install

CsrpServerSetup/Releaseにあるインストーラを実行

# Linux版サーバー

コンソールアプリケーションです。Monoとlibusb-1.0が必要です。サーバーがEZ-USB FX2LPのRAMにファームウェアを書き込むのでEEPROMのジャンパーは外しておきます。

## Install

MonoCsrpServer/bin/MonoCsrpServerディレクトリごとコピーして
```
$ sudo mono MonoCsrpServer.exe [サンプルレートの初期値(kHz)]
```
  
# Authors

[Rapidnack](http://rapidnack.com/)

# References

[https://github.com/Rapidnack/Fx2lpSdrServer](https://github.com/Rapidnack/Fx2lpSdrServer)

[https://github.com/Rapidnack/gr-rapidnack](https://github.com/Rapidnack/gr-rapidnack)

[https://github.com/Rapidnack/Spi1FifoIn](https://github.com/Rapidnack/Spi1FifoIn)

[https://github.com/Rapidnack/MAX1000_SDR_FX2LP](https://github.com/Rapidnack/MAX1000_SDR_FX2LP)

[https://github.com/Rapidnack/MAX10FB_SDR_FX2LP](https://github.com/Rapidnack/MAX10FB_SDR_FX2LP)

[https://github.com/Rapidnack/CYC4_SDR_FX2LP](https://github.com/Rapidnack/CYC4_SDR_FX2LP)

[https://github.com/Rapidnack/CYC2_SDR_FX2LP](https://github.com/Rapidnack/CYC2_SDR_FX2LP)
