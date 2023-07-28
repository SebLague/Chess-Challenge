#!/bin/bash -e

if grep -q 'Arch' /etc/os-release; then
    unset DBUS_SESSION_BUS_ADDRESS
fi

if [ -z "$WAYLAND_DISPLAY" ]; then
    CRI_OPT+=(
        --network host
        -e XAUTHORITY=/app/.Xauthority
        -v "$XAUTHORITY:/app/.Xauthority:ro"
    )
fi

CRI="$(command -v podman || command -v docker)"

if ! "${CRI[@]}" image ls &>/dev/null; then
    CRI=(sudo "${CRI[@]}")
fi

"${CRI[@]}" build . -t chess

"${CRI[@]}" run --rm --name chess          \
    "${CRI_OPT[@]}"                        \
    --device /dev/dri/                     \
    -e DISPLAY                             \
    -e XDG_RUNTIME_DIR                     \
    -v /dev/shm/:/dev/shm/                 \
    -v /tmp/.X11-unix/:/tmp/.X11-unix/     \
    -v "$XDG_RUNTIME_DIR:$XDG_RUNTIME_DIR" \
    chess
