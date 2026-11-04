#!/usr/bin/env python3
"""Serve a topic folder on 127.0.0.1 with caching disabled, for checking a freshly built app.

Usage: serve.py TOPIC_DIR [--port 8765] [--host 127.0.0.1]

Use --host 0.0.0.0 to reach it from other devices on the same network (e.g. an iPad).

Plain `python -m http.server` sends Last-Modified without Cache-Control, so browsers apply
heuristic freshness and keep showing the previous build after a rebuild. This sends no-store.
"""
import argparse
import functools
import http.server


class NoCache(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header("Cache-Control", "no-store")
        super().end_headers()

    def log_message(self, *args):
        pass


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("topic_dir")
    ap.add_argument("--port", type=int, default=8765)
    ap.add_argument("--host", default="127.0.0.1", help="0.0.0.0 to allow other devices on the network")
    a = ap.parse_args()
    handler = functools.partial(NoCache, directory=a.topic_dir)
    with http.server.ThreadingHTTPServer((a.host, a.port), handler) as srv:
        print(f"Serving {a.topic_dir} on {a.host}:{a.port} (Ctrl+C to stop)", flush=True)
        srv.serve_forever()


if __name__ == "__main__":
    main()
