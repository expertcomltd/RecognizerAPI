from keras.models import load_model	
import numpy as np
import cv2
import sys
import time
import pickle
import os
import tensorflow as tf
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler
from keras.preprocessing import image
import json

def load_obj(name):
    with open(name, 'rb') as f:
        return pickle.load(f)

def load_trained(path):
	model = load_model(os.path.join(path,'fer2013_emotions.h5'))
	print("Loaded model from disk")
	return model

# Face detection functions

def initFacesDLIB():
	print("initFacesDLIB")

def detectFacesDLIB(path):
	import face_recognition
	frame = cv2.imread(path);
	gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
	rgb_frame = frame[:, :, ::-1]
	face_locations = face_recognition.face_locations(rgb_frame)
	faceresult = []
	for (top, right, bottom, left) in face_locations:
		rx = left
		ry = top
		rw = right-left
		rh = bottom-top

		img = gray[ry:ry+rh,rx:rx+rw]
		img = cv2.resize(img, (48, 48))
		img = img.reshape((48,48,1))
		img = img/255.0
		img = np.expand_dims(img,axis=0)
		predclass = model.predict_classes(img)
		idx = predclass[0]
		emotion_id = predclass[0]
		
		emotion = '';

		for name, emotionid in categories.items():
			if emotionid == emotion_id:
				emotion = name
				break


		#print("X:"+ str(rx) + ",Y:" + str(ry) +",W:" + str(rw) +",H:" + str(rh) +",emotion_id:" + str(emotion_id))

		faceresult.append({ 'X':rx,'Y':ry,'W':rw,'H':rh,'Emotion':emotion})
	#print(faceresult)
	return faceresult


def initFacesMTCNN():
	print("initFacesMTCNN")
	from mtcnn.mtcnn import MTCNN
	global detector
	detector = MTCNN()

def detectFacesMTCNN(path):
	frame = cv2.imread(path);
	gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
	faces = detector.detect_faces(frame)
	count = 0
	faceresult = []
	
	for face in faces:
		#print(face)
		rx, ry, rw, rh = face['box']
		keypoints = face['keypoints']
		lex, ley = keypoints['left_eye']
		rex, rey = keypoints['right_eye']

		img = gray[ry:ry+rh,rx:rx+rw]
		img = cv2.resize(img, (48, 48))
		img = img.reshape((48,48,1))
		img = img/255.0
		img = np.expand_dims(img,axis=0)
		predclass = model.predict_classes(img)
		idx = predclass[0]
		emotion_id = predclass[0]
		
		emotion = '';

		for name, emotionid in categories.items():
			if emotionid == emotion_id:
				emotion = name
				break


		print("X:"+ str(rx) + ",Y:" + str(ry) +",W:" + str(rw) +",H:" + str(rh) +",emotion_id:" + str(emotion_id))


		faceresult.append({ 'X':rx,'Y':ry,'W':rw,'H':rh,'E':emotion,'REX':rex,'REY':rey,'LEX':lex,'LEY':ley})
	#print(faceresult)
	return faceresult

src_path = ''

if len(sys.argv)<3:
	raise IOError("Missing arguments")

src_path = sys.argv[1];
detectmode = int(sys.argv[2]);
	
model = load_trained(src_path)

graph = tf.get_default_graph()

categories = load_obj(os.path.join(src_path,'emotions_categories.pkl'))

if detectmode==0:
	initFacesMTCNN()
else:
	initFacesDLIB()

print("Categories loaded:")
print(categories)

class ImagesWatcher:
    def __init__(self, src_path):
        self.__emotion_src_path = os.path.join(src_path,"emotiondetect")
        self.__facedetect_src_path = os.path.join(src_path,"facedetect")
        self.__emotion_event_handler = EmotionImagesEventHandler()
        self.__facedetect_event_handler = FaceDetectImagesEventHandler()
        self.__event_observer = Observer()

    def run(self):
        self.start()
        try:
            while True:
                time.sleep(1)
        except KeyboardInterrupt:
            self.stop()

    def start(self):
        self.__schedule()
        self.__event_observer.start()

    def stop(self):
        self.__event_observer.stop()
        self.__event_observer.join()

    def __schedule(self):
        self.__event_observer.schedule(
            self.__emotion_event_handler,
            self.__emotion_src_path,
            recursive=True
        )
        self.__event_observer.schedule(
            self.__facedetect_event_handler,
            self.__facedetect_src_path,
            recursive=True
        )

class EmotionImagesEventHandler(PatternMatchingEventHandler):
	IMAGES_REGEX = [r"\.png$"]

	def __init__(self):
		super().__init__(patterns=["*.jpg", "*.jpeg", "*.png", "*.bmp"])

	def on_created(self, event):
		self.process(event)

	def process(self, event):
		global graph
		with graph.as_default():	
			try:
				print("processing " + event.src_path)
				filename, ext = os.path.splitext(event.src_path)
				
				time.sleep(0.2)
				
				image_img = cv2.imread(event.src_path,0)
				image_img = cv2.resize(image_img, (48, 48))
				image_img = image_img.reshape((48,48,1))
				
				image_img = image_img/255.0
				image_img = np.expand_dims(image_img,axis=0)
				
				predclass = model.predict_classes(image_img)
				emotion_id = predclass[0]
				print("emotion_id ",emotion_id)
				
				emotion = '';
			
				for name, emotionid in categories.items():
					if emotionid == emotion_id:
						emotion = name
						break
				
				ofilepath = filename + '.txt'
				
				print("Saving " + ofilepath)
				file = open(ofilepath,"w") 
				file.write(str(emotion_id) + ":" + emotion)
				file.close();
			except:
				print("ERROR: ",sys.exc_info())

class FaceDetectImagesEventHandler(PatternMatchingEventHandler):
	IMAGES_REGEX = [r"\.png$"]

	def __init__(self):
		super().__init__(patterns=["*.jpg", "*.jpeg", "*.png", "*.bmp"])

	def on_created(self, event):
		self.process(event)

	def process(self, event):
		global graph
		with graph.as_default():	
			try:
				print("processing " + event.src_path)
				filename, ext = os.path.splitext(event.src_path)
				time.sleep(0.2)
				faces = []
				if detectmode==0:
					faces = detectFacesMTCNN(event.src_path)
				else:
					faces = detectFacesDLIB(event.src_path)

				ofilepath = filename + '.json'
				print("Saving " + ofilepath)
				file = open(ofilepath,"w") 
				file.write(json.dumps(faces))
				file.close();
			except:
				print("ERROR: ",sys.exc_info())


print("Starting ImagesWatcher")
ImagesWatcher(src_path).run()
