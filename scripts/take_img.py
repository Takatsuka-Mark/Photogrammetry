from time import sleep
from picamera import PiCamera


def main():
    print("Booting camera")
    cam = PiCamera()
    # cam.resolution = (1920, 1080)
    cam.resolution = (2560, 1440)
    cam.start_preview()
    sleep(2)
    cam.capture(f'./data/test.jpg')

if __name__ == '__main__':
    main()
