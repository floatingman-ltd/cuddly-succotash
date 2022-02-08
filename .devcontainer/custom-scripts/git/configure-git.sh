#! /bin/bash

mkdir t
cd t
git init
git config --global core.editor "vi"
git config --global user.name "waltiam"
git config --global user.email "2963975+waltiam@users.noreply.github.com"

## gpgsm signing
# git config --global user.signingkey $(gpgsm --list-secret-keys | awk 'match($0,/0x/) {id =  substr($0, RSTART, 10)}END{print id}')
# git config --global gpg.program $(which gpgsm)
# git config --global gpg.format x509

git config --global user.signingkey $(gpg --list-secret-keys --keyid-format 0xlong | awk 'match($0,/0x/) {id =  substr($0, RSTART+2, 16)}END{print id}')
git config --global gpg.program $(which gpg)

git config --global commit.gpgSign true
git config --global tag.gpgSign true

# go by default uses 'https'
git config --global url."git@github.com:".insteadOf "https://github.com/"
cd ..
rm -rf t

echo "Cleaned up"


# gah - silly terminal
# export GPG_TTY=$(tty)
# gpgconf --kill gpg-agent
# sourced from:
# https://tutorials.technology/solved_errors/21-gpg-signing-failed-Inappropriate-ioctl-for-device.html
# echo 'export GPG_TTY=$(tty)' >> ${USER_RC_PATH}/.zshrc
