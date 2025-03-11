/*
 * HmTinyHttpServer v 1.1.1.2
 * This codes is licensed under CC0 1.0 Universal
 */ 

var createTinyHttpServer;

(function () {
    // 定義済みなら何もしない
    if (typeof(createTinyHttpServer) == "function") {
        return;
    }

    // 基本はexecjsから呼び出すが、理論上はこの.js全体をevalすることも可能ではある。
    var currentJsFileName = hidemaruGlobal.currentjsfilename() || hidemaruGlobal.currentmacrofilename();
    var splitted = currentJsFileName.split("\\");
    splitted.pop();
    var currentJsDirectory = splitted.join("\\");

    // エラー用途
    function output(msg) {
        var dllobj = hidemaru.loadDll("HmOutputPane.dll");
        return dllobj.dllFuncW.OutputW(hidemaru.getCurrentWindowHandle(), msg + "\r\n");
    }

    // クラス的なもの。コンストラクタ的なもの
    // props { rootFolder: "C:\\path\\to\\folder" }
    createTinyHttpServer = function (props) {

        if (!props) {
            output("引数がありません");
            return null;
        }

        if (!props.rootFolder) {
            output("rootFolderが指定されていません");
            return null;
        }

        // それ「ディレクトリ」なんだろうな？ おん？
        var is_directory = hidemaruGlobal.existfile(props.rootFolder, 1) & 0x00000010;
        if (!is_directory) {
            output("「" + props.rootFolder + "」というフォルダ存在しません。");
            return null;
        }

        // 実質的にユーザが利用すうるインスタンスのメソッド
        return {

            processInfo: null,

            start: function () {
                // コマンドライン構築
                var command = hidemaruGlobal.sprintf('%s\\%s "%s" %d', currentJsDirectory, "\\HmTinyHttpServer.exe", props.rootFolder, hidemaru.getCurrentWindowHandle());

                // 実行してみる
                this.processInfo = hidemaru.runProcess(command, ".", "stdioAlive", "utf8");
                if (this.processInfo) {

                    // ポート番号が出てくるので待つ。0.1～0.2秒程度なので、以下のように同期的に待っても影響は少ないいが、不安なら非同期のonReadLineの書き方でもよい。
                    var output = this.processInfo.stdOut.readLine(1000);

                    var port = Number(output);
                    if (port > 0) {
                        return port;
                    }
                }

                return 0;
            },

            startAsync: function (onPort) {
                // コマンドライン構築
                var command = hidemaruGlobal.sprintf('%s\\%s "%s" %d', currentJsDirectory, "\\HmTinyHttpServer.exe", props.rootFolder, hidemaru.getCurrentWindowHandle());

                var sendPort = false;

                // 実行してみる
                this.processInfo = hidemaru.runProcess(command, ".", "stdioAlive", "utf8");
                if (this.processInfo) {

                    this.processInfo.stdOut.onReadLine(function (output) {
                        if (sendPort) { return; }
                        var port = Number(output);
                        if (port > 0) {
                            onPort(port);
                        } else {
                            onPort(0);
                        }
                        sendPort = true;
                    }
                    );
                } else {
                    if (sendPort) { return; }
                    onPort(0);
                    sendPort = true;
                }
            },

            close: function () {
                // 前回この「currentmacrofilename」でのjs実行空間内でサーバーを立ち上げてるなら終了
                if (this.processInfo) {
                    this.processInfo.stdIn.writeLine("exit");
                }
            }
        };
    }

})();