from flask import Flask, Response
from picamera import PiCamera
from io import BytesIO
from threading import Condition
from time import sleep

app = Flask(__name__)

class StreamingOutput(object):
    def __init__(self):
        self.frame = None
        self.buffer = BytesIO()
        self.condition = Condition()

    def write(self, buf):
        if buf.startswith(b'\xff\xd8'):
            # New frame, copy the existing buffer's content and notify all
            # clients it's available
            self.buffer.truncate()
            with self.condition:
                self.frame = self.buffer.getvalue()
                self.condition.notify_all()
            self.buffer.seek(0)
        return self.buffer.write(buf)

@app.route("/")
def hello_world():
    return "<p>Hello World!</p>"

def get_frame():
    while True:
        with output.condition:
            output.condition.wait()
            frame = output.frame
        yield (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + frame + b'\r\n')

@app.route('/video-feed')
def video_feed():
    return Response(get_frame(),
                    mimetype='multipart/x-mixed-replace; boundary=frame')

if __name__ == '__main__':
    with PiCamera(resolution='1920x1080', framerate=10) as camera:
        sleep(2)
        global output
        output = StreamingOutput()
        camera.start_recording(output, format='mjpeg')
        try:
            app.run(host='0.0.0.0')
        finally:
            camera.stop_recording()
