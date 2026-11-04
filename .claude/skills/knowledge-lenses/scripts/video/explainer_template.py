"""Explainer video template: Manim scenes narrated with local Kokoro TTS and burned-in subtitles.

Copy this file into the topic folder as <topic>-explainer.py, keep the helpers, and replace the
beats in Explainer.construct. Render with:

  VIDEO_ENV=<env dir> <env>/.venv/bin/manim -ql --media_dir <tmp>/media --disable_caching <file> Explainer   # draft
  ... -r 1920,1080 --frame_rate 30 ...                                                                         # final

VIDEO_ENV is the directory set up by setup_video_env.sh (default ~/.cache/knowledge-lenses/video-env).
"""
import hashlib
import os
import textwrap
import wave
from pathlib import Path

from manim import (BOLD, DOWN, GREY_B, RIGHT, UP, Arrow, FadeIn, FadeOut, GrowArrow, Indicate,
                   RoundedRectangle, Scene, Text, VGroup, Write)

VOICE = "af_heart"          # also good: af_bella, am_michael (US male), bm_george (British male)
SPEED = 1.0
FONT = "Helvetica Neue"
ENV = Path(os.environ.get("VIDEO_ENV", Path.home() / ".cache/knowledge-lenses/video-env"))
AUDIO_DIR = Path(__file__).parent / ".narration-cache"
AUDIO_DIR.mkdir(exist_ok=True)

YELLOW = "#E8C547"
BLUE = "#58C4DD"
GREEN = "#83C167"
RED = "#FC6255"
ORANGE = "#F0A04B"
GREY = "#888888"

_engine = {}


def _kokoro():
    if not _engine:
        from kokoro_onnx import Kokoro
        from misaki import en
        _engine["g2p"] = en.G2P(trf=False, british=False, fallback=None)
        _engine["tts"] = Kokoro(str(ENV / "kokoro/kokoro-v1.0.onnx"), str(ENV / "kokoro/voices-v1.0.bin"))
    return _engine["g2p"], _engine["tts"]


def tts(spoken):
    """Synthesize once per distinct line (cached), return (wav path, seconds)."""
    key = hashlib.sha1(f"{VOICE}{SPEED}{spoken}".encode()).hexdigest()[:12]
    path = AUDIO_DIR / f"{key}.wav"
    if not path.exists():
        import soundfile as sf
        g2p, engine = _kokoro()
        phonemes, _ = g2p(spoken)
        if "❓" in phonemes:
            print(f"WARNING unknown word, rephrase or respell in `spoken`: {spoken!r} -> {phonemes}")
        samples, sr = engine.create(phonemes, voice=VOICE, speed=SPEED, is_phonemes=True)
        sf.write(str(path), samples, sr, subtype="PCM_16")
    with wave.open(str(path)) as w:
        return str(path), w.getnframes() / w.getframerate()


def box(label, color=BLUE, w=2.8, h=1.1, fs=24):
    rect = RoundedRectangle(corner_radius=0.15, width=w, height=h,
                            stroke_color=color, fill_color=color, fill_opacity=0.15)
    t = Text(label, font=FONT, font_size=fs, line_spacing=0.8).move_to(rect)
    if t.width > w - 0.3:
        t.scale_to_fit_width(w - 0.3)
    return VGroup(rect, t)


class NarratedScene(Scene):
    """line() plays narration for one sentence, shows it as a subtitle, runs the animations,
    then waits until the audio ends, so picture and voice stay in sync."""

    def setup(self):
        self.sub = None

    def line(self, subtitle, spoken=None, fn=None):
        path, dur = tts(spoken or subtitle)
        self.add_sound(path)
        if self.sub is not None:
            self.remove(self.sub)
        wrapped = "\n".join(textwrap.wrap(subtitle, 78))
        self.sub = Text(wrapped, font=FONT, font_size=22, color=GREY_B, line_spacing=0.8).to_edge(DOWN, buff=0.3)
        self.add(self.sub)
        t0 = self.renderer.time
        if fn:
            fn()
        elapsed = self.renderer.time - t0
        self.wait(max(0.2, dur - elapsed + 0.3))

    def clear_stage(self):
        mobs = [m for m in self.mobjects if m is not self.sub]
        if mobs:
            self.play(*[FadeOut(m) for m in mobs], run_time=0.6)

    def end_card(self, text):
        if self.sub is not None:
            self.remove(self.sub)
            self.sub = None
        end = Text(text, font=FONT, font_size=40)
        self.play(FadeIn(end), run_time=0.8)
        self.wait(1.5)
        self.play(FadeOut(end), run_time=0.8)


class Explainer(NarratedScene):
    def construct(self):
        # Beat 1: title. Replace each beat with the topic's own; 5-7 beats, 2-5 lines each.
        title = Text("Topic", font=FONT, font_size=96, weight=BOLD)
        self.line("This is what the topic is, in one sentence.",
                  fn=lambda: self.play(Write(title), run_time=1.5))
        self.play(title.animate.scale(0.45).to_edge(UP, buff=0.4), run_time=0.8)

        # Beat 2: a flow, built up one sentence at a time.
        a, b = box("Input", GREY), box("Result", YELLOW)
        VGroup(a, b).arrange(RIGHT, buff=1.5)
        arrow = Arrow(a.get_right(), b.get_left(), buff=0.1, color=GREY_B)

        def flow():
            self.play(FadeIn(a, shift=RIGHT * 0.2), run_time=0.7)
            self.play(GrowArrow(arrow), FadeIn(b, scale=0.8), run_time=0.9)
            self.play(Indicate(b, color=YELLOW), run_time=0.8)

        self.line("Something goes in, and something comes out.", fn=flow)
        self.clear_stage()
        self.end_card("Topic · subtitle")
