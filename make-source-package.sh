#!/bin/bash

make vcs_version
debuild -uc -us -S -nc -sa
