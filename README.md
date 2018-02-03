# KinectSkeletonTracking

//間違ってたら教えて。

各自の作業用ブランチは 「develop/xxxx(自分の名前)」 とする。<br>
masterへの直接pushは禁止。

# 環境構築

## ローカル（自分のPC）に作業用ディレクトリを作成しそこにcloneする。
` git clone https://github.com/HnTeam2/KinectSkeletonTracking.git `<br>
` cd KinectSkeletonTracking `<br>

## 自分の作業用ブランチを切る

現在のブランチがmasterであることを確認。
` git branch `<br>
新しくブランチを作成して移動する。
` git checkout -b develop/xxxx(自分の名前) `<br>
↑で切ったブランチに移動しているか確認する
` git branch `<br>

## Pushする時。
変更部分を確認。<br>
` git diff `<br>
` git add . `<br>
` git commit -m "メッセージ" `<br>
` git push origin develop/xxxx(自分の名前) `<br>
