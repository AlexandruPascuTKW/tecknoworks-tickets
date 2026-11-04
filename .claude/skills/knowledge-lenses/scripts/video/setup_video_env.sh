#!/usr/bin/env bash
# Create (or reuse) a self-contained video toolchain: Manim for animation, Kokoro for local narration.
# Usage: setup_video_env.sh VIDEO_ENV_DIR
# Everything lands inside VIDEO_ENV_DIR (venv, model files, pkg-config stubs); nothing is installed system-wide.
# Needs: python3 3.10-3.12, ffmpeg, cairo + pango libraries (macOS: Homebrew), and `uv` or `python3 -m venv`.
set -euo pipefail

ENV_DIR="${1:?usage: setup_video_env.sh VIDEO_ENV_DIR}"
mkdir -p "$ENV_DIR"
cd "$ENV_DIR"

command -v ffmpeg >/dev/null || { echo "ffmpeg missing (macOS: brew install ffmpeg)"; exit 1; }

if [ ! -x .venv/bin/python ]; then
  if command -v uv >/dev/null; then uv venv -q .venv --python 3.12; else python3 -m venv .venv; fi
fi
PY="$PWD/.venv/bin/python"
pip_install() {
  if command -v uv >/dev/null; then uv pip install -q --python "$PY" "$@"; else "$PY" -m pip install -q "$@"; fi
}

if [ "$(uname)" = "Darwin" ]; then
  # pycairo builds from source and needs pkg-config. The PyPI pkgconf keeps it inside the venv.
  # Homebrew's cairo .pc files require zlib/bzip2/expat/libffi .pc files; macOS ships those libraries
  # in the SDK without pkg-config metadata, so write small stubs that point at the SDK.
  pip_install pkgconf
  [ -e .venv/bin/pkg-config ] || ln -s pkgconf .venv/bin/pkg-config
  SDK="$(xcrun --show-sdk-path)"
  mkdir -p pcstub
  for spec in "bzip2:-lbz2:" "expat:-lexpat:" "libffi:-lffi:/ffi" "zlib:-lz:"; do
    n=${spec%%:*}; rest=${spec#*:}; lib=${rest%%:*}; inc=${rest#*:}
    printf 'Name: %s\nDescription: macOS SDK %s\nVersion: 99\nLibs: -L%s/usr/lib %s\nCflags: -I%s/usr/include%s\n' \
      "$n" "$n" "$SDK" "$lib" "$SDK" "$inc" > "pcstub/$n.pc"
  done
  BREW="$(brew --prefix 2>/dev/null || echo /opt/homebrew)"
  KEGS="$(ls -d "$BREW"/opt/*/lib/pkgconfig 2>/dev/null | tr '\n' ':')"
  export PATH="$PWD/.venv/bin:$PATH"
  export PKG_CONFIG_PATH="$BREW/lib/pkgconfig:$BREW/share/pkgconfig:$KEGS$PWD/pcstub"
  pkg-config --cflags cairo pangocairo >/dev/null || { echo "cairo/pango not found (brew install cairo pango)"; exit 1; }
fi

"$PY" -c "import manim" 2>/dev/null || pip_install manim
# misaki does grapheme-to-phoneme itself. The espeak-ng bundled with kokoro-onnx on macOS ignores its
# data path and fails, so narration goes text -> misaki phonemes -> kokoro (is_phonemes=True).
"$PY" -c "import kokoro_onnx, soundfile, misaki" 2>/dev/null || pip_install kokoro-onnx soundfile "misaki[en]"
"$PY" -c "import en_core_web_sm" 2>/dev/null || pip_install \
  "https://github.com/explosion/spacy-models/releases/download/en_core_web_sm-3.8.0/en_core_web_sm-3.8.0-py3-none-any.whl"

mkdir -p kokoro
for f in kokoro-v1.0.onnx voices-v1.0.bin; do
  [ -s "kokoro/$f" ] || curl -sSL -o "kokoro/$f" "https://github.com/thewh1teagle/kokoro-onnx/releases/download/model-files-v1.0/$f"
done

"$PY" - <<'EOF'
import manim, kokoro_onnx, misaki
print("video env ready: manim", manim.__version__)
EOF
echo "Python: $PY"
