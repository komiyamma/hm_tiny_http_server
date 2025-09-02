# HmTinyHttpServer

![HmTinyHttpServer latest release](https://img.shields.io/github/v/release/komiyamma/hm_tiny_http_server)
[![MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)
![Hidemaru 9.25](https://img.shields.io/badge/Hidemaru-v9.25-6479ff.svg)

「秀丸エディタ」で「ブラウザ枠」や「レンダリング枠」に http://localhost:port/  
形式でアクセスできるようにするためのコンポーネントです。

https://秀丸マクロ.net/?page=nobu_tool_hm_tiny_http_server

---

## 機能概要

`HmTinyHttpServer.exe` は、指定されたフォルダをドキュメントルートとして、PHPのビルトインWebサーバーを起動するための軽量なラッパーツールです。秀丸エディタから利用されることを想定しており、秀丸エディタのプロセスを監視し、エディタが終了するとWebサーバーも自動的に終了する仕組みを持っています。

## 主な機能

### 1. PHPビルトインサーバーの起動

内部に同梱されている `php.exe` を利用して、`php -S localhost:{port} -t "{document_root}"` コマンドを実行し、Webサーバーを起動します。

### 2. 空きポートの自動検出

サーバーを起動する際、利用可能なTCPポートを自動的に検索して使用します。これにより、ポート番号の衝突を気にする必要がありません。起動したサーバーのポート番号は、標準出力へ出力されます。

### 3. 多重起動の防止

セマフォを利用して、同時に複数のサーバーインスタンスが起動しないように制御されています。既にサーバーが起動している場合は、新しいプロセスは何もせずに終了します。

### 4. 秀丸エディタとの連携と自動終了

このツールは、起動時にコマンドライン引数として秀丸エディタのウィンドウハンドルを受け取ります。
1秒ごとにそのウィンドウハンドルが存在するかどうかを監視し、存在しなくなった場合（＝秀丸エディタが終了した場合）は、起動したPHPサーバーを自動的に停止させ、自身のプロセスも終了します。

これにより、不要なプロセスが残り続けるのを防ぎます。

## 起動方法

コマンドプロンプトや秀丸マクロの `run` や `runex` コマンド等から、以下のように実行します。

```
HmTinyHttpServer.exe "ドキュメントルートのフルパス" "秀丸エディタのウィンドウハンドル"
```

- **第1引数**: Webサーバーのドキュメントルートとなるフォルダの絶対パスを指定します。
- **第2引数**: 監視対象とする秀丸エディタのウィンドウハンドル（10進数の数値）を指定します。

**実行例:**
```
HmTinyHttpServer.exe "C:\Users\user\Documents\MyWebApp" "131584"
```
