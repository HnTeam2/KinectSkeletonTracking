# KinectSkeletonTracking

//間違ってたら教えて。

各自の作業用ブランチは 「develop/xxxx(自分の名前)」 とする。
masterへの直接pushは禁止。

# 環境構築

## ローカル（自分のPC）に作業用ディレクトリを作成しそこにcloneする。
` git clone https://github.com/HnTeam2/KinectSkeletonTracking.git `
` cd KinectSkeletonTracking `

## 自分の作業用ブランチを切る

現在のブランチがmasterであることを確認。
` git branch `
新しくブランチを作成して移動する。
` git checkout -b develop/xxxx(自分の名前) `
↑で切ったブランチに移動しているか確認する
` git branch `

## Pushする時。
変更部分を確認。
` git diff `
` git add . `
` git commit -m "メッセージ" `
` git push origin develop/xxxx(自分の名前) `
