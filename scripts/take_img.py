from time import sleep
from picamera import PiCamera


def main():
    print("Booting camera")
    cam = PiCamera()
    cam.resolution = (1920, 1080)
    # cam.start_preview()
    sleep(2)
    for i in range(100):
        cam.capture(f'./data/test{i}.jpg')
        sleep(0.5)
    # print("Image Captured")


if __name__ == '__main__':
    main()
