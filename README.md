# CsrpServer

CsrpServerプロジェクトは、Intel FPGAとCypress EZ-USB FX2LPを使ったCSRP(Cheap Software Radio Peripheral)のWindows版サーバーとLinux版サーバーのソースコードです。
ExtIO_USRPと通信するため、BorIPプロトコルを実装しています。

![構成図](http://rapidack.sakura.ne.jp/ttl/wp-content/uploads/2019/05/CsrpServer2.png)

## Windows版サーバー

WinFormsアプリケーションです。サーバーがEZ-USB FX2LPのRAMにファームウェアを書き込むのでEEPROMのジャンパーは外しておきます。

![Windows版サーバー](http://rapidack.sakura.ne.jp/ttl/wp-content/uploads/2019/05/CsrpServer.png)

### Install

CsrpServerSetup/Releaseにあるインストーラを実行

## Linux版サーバー

コンソールアプリケーションです。Monoとlibusb-1.0が必要です。サーバーがEZ-USB FX2LPのRAMにファームウェアを書き込むのでEEPROMのジャンパーは外しておきます。

### Install

MonoCsrpServer/bin/MonoCsrpServerディレクトリごとコピー
```
$ sudo mono MonoCsrpServer.exe [サンプルレートの初期値(kHz)]
```
  
# FPGA/MAX1000_SDR_FX2LP

MAX1000_SDR_FX2LPプロジェクトは、Intel MAX10 FPGAとCypress EZ-USB FX2LPを使ったCSRP(Cheap Software Radio Peripheral)のVerilogソースコードです。

![全体画像](http://rapidack.sakura.ne.jp/ttl/wp-content/uploads/2019/12/882f834b9c48e1777c1405197df7e3ae.png)

- 開発環境: Quartus Prime Lite Edition 18.1
- FPGAボード: Arrow Development Tools MAX1000
- USBコントローラ: Cypress EZ-USB FX2LP
- ADC: Analog Devices AD9283

# FPGA/MAX10FB_SDR_FX2LP

MAX10FB_SDR_FX2LPプロジェクトは、Intel MAX10 FPGAとCypress EZ-USB FX2LPを使ったCSRP(Cheap Software Radio Peripheral)のVerilogソースコードです。

![全体画像](http://rapidack.sakura.ne.jp/ttl/wp-content/uploads/2019/12/4412da12d94d7c36d5709442a01ff044.png)

- 開発環境: Quartus Prime Lite Edition 18.1
- FPGAボード: CQ出版社「FPGA電子工作スーパーキット」付録基板 MAX10-FB
- USBコントローラ: Cypress EZ-USB FX2LP
- ADC: Analog Devices AD9283

# FPGA/CYC4_SDR_FX2LP

CYC4_SDR_FX2LPプロジェクトは、Intel Cyclone IV FPGAとCypress EZ-USB FX2LPを使ったCSRP(Cheap Software Radio Peripheral)のVerilogソースコードです。

![全体画像](http://rapidack.sakura.ne.jp/ttl/wp-content/uploads/2019/12/5b483b41de63a94c17ae92bd770cbff0.png)

- 開発環境: Quartus Prime Lite Edition 18.1
- FPGAボード: EP4CE6E22C8N Development Board
- USBコントローラ: Cypress EZ-USB FX2LP
- ADC: Analog Devices AD9283

# FPGA/CYC2_SDR_FX2LP

CYC2_SDR_FX2LPプロジェクトは、Intel Cyclone II FPGAとCypress EZ-USB FX2LPを使ったCSRP(Cheap Software Radio Peripheral)のVerilogソースコードです。

![全体画像](http://rapidack.sakura.ne.jp/ttl/wp-content/uploads/2019/12/42b9b27ba2b2db472bc8d07555511c9c.png)

- 開発環境: Quartus II 13.0sp1 Web Edition
- FPGAボード: EP2C5T144 Development Board
- USBコントローラ: Cypress EZ-USB FX2LP
- ADC: Analog Devices AD9283

# Authors

[Rapidnack](http://rapidnack.com/)

# References

[https://github.com/Rapidnack/CsrpServer](https://github.com/Rapidnack/CsrpServer)

[https://github.com/Rapidnack/gr-rapidnack](https://github.com/Rapidnack/gr-rapidnack)

[https://github.com/Rapidnack/Spi1FifoIn](https://github.com/Rapidnack/Spi1FifoIn)

[https://github.com/Rapidnack/MAX1000_SDR_FX2LP](https://github.com/Rapidnack/MAX1000_SDR_FX2LP)

[https://github.com/Rapidnack/MAX10FB_SDR_FX2LP](https://github.com/Rapidnack/MAX10FB_SDR_FX2LP)

[https://github.com/Rapidnack/CYC4_SDR_FX2LP](https://github.com/Rapidnack/CYC4_SDR_FX2LP)

[https://github.com/Rapidnack/CYC2_SDR_FX2LP](https://github.com/Rapidnack/CYC2_SDR_FX2LP)
