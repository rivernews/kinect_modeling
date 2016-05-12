#include <Windows.h>
#include <Ole2.h>

#include <gl/GL.h>
#include <gl/GLU.h>
#include <gl/glut.h>

// #include <glew.h>

#include <NuiApi.h>
#include <NuiImageCamera.h>
#include <NuiSensor.h>

#define width 640
#define height 480

// OpenGL Variables
GLuint textureId;              // ID of the texture to contain Kinect RGB Data
GLubyte data[width*height * 4];  // BGRA array containing the texture data

								 // Kinect variables
HANDLE rgbStream;              // The identifier of the Kinect's RGB Camera
HANDLE depthStream;
INuiSensor* sensor;            // The kinect sensor



void getKinectData(GLubyte* dest) {
	// a structure containing all the metadata about the frame - the number, resolution, etc. 
	NUI_IMAGE_FRAME imageFrame;
	// pointer to the actual data
	NUI_LOCKED_RECT LockedRect;
	// acquire a NUI_IMAGE_FRAME from the HANDLE we initialized earlier
	if (sensor->NuiImageStreamGetNextFrame(rgbStream, 0, &imageFrame) < 0) return;
	// get an INuiFrameTexture so that we can get the pixel data out of it, using a NUI_LOCKED_RECT
	INuiFrameTexture* texture = imageFrame.pFrameTexture;
	texture->LockRect(0, &LockedRect, NULL, 0);

	// copy the data to our own memory location
	// The Pitch of the LockedRect is how many bytes are in each row of the frame
	// a simple check on that value makes sure that the frame is not empty
	if (LockedRect.Pitch != 0)
	{
		const BYTE* curr = (const BYTE*)LockedRect.pBits;
		const BYTE* dataEnd = curr + (width*height) * 4;

		while (curr < dataEnd) {
			*dest++ = *curr++;
		}
	}
	// The Kinect data is in BGRA format, so we can copy it directly into our buffer and use it as an OpenGL texture

	// Finally, we have to release the frame so that the Kinect can use it again
	texture->UnlockRect(0);
	sensor->NuiImageStreamReleaseFrame(rgbStream, &imageFrame);
}
void getKinectDataDepth(GLubyte* dest) {
	// a structure containing all the metadata about the frame - the number, resolution, etc. 
	NUI_IMAGE_FRAME imageFrame;
	// pointer to the actual data
	NUI_LOCKED_RECT LockedRect;
	// acquire a NUI_IMAGE_FRAME from the HANDLE we initialized earlier
	if (sensor->NuiImageStreamGetNextFrame(depthStream, 0, &imageFrame) < 0) return;
	// get an INuiFrameTexture so that we can get the pixel data out of it, using a NUI_LOCKED_RECT
	INuiFrameTexture* texture = imageFrame.pFrameTexture;
	texture->LockRect(0, &LockedRect, NULL, 0);

	// copy the data to our own memory location
	// The Pitch of the LockedRect is how many bytes are in each row of the frame
	// a simple check on that value makes sure that the frame is not empty
	if (LockedRect.Pitch != 0)
	{
		const USHORT* curr = (const USHORT*)LockedRect.pBits;
		const USHORT* dataEnd = curr + (width*height);

		while (curr < dataEnd) {
			// Get depth in millimeters
			USHORT depth = NuiDepthPixelToDepth(*curr++);

			// Draw a grayscale image of the depth:
			// B,G,R are all set to depth%256, alpha set to 1.
			for (int i = 0; i < 3; ++i)
				*dest++ = (BYTE)depth % 256;
			*dest++ = 0xff;
		}
	}
	// The Kinect data is in BGRA format, so we can copy it directly into our buffer and use it as an OpenGL texture

	// Finally, we have to release the frame so that the Kinect can use it again
	texture->UnlockRect(0);
	sensor->NuiImageStreamReleaseFrame(depthStream, &imageFrame);
}

// glut window handling
// Drawing a frame to screen
void drawKinectData() {
	glBindTexture(GL_TEXTURE_2D, textureId);
	getKinectData(data);
	glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, width, height, GL_BGRA_EXT, GL_UNSIGNED_BYTE, (GLvoid*)data);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	glBegin(GL_QUADS);
	glTexCoord2f(0.0f, 0.0f);
	glVertex3f(0, 0, 0);
	glTexCoord2f(1.0f, 0.0f);
	glVertex3f(width, 0, 0);
	glTexCoord2f(1.0f, 1.0f);
	glVertex3f(width, height, 0.0f);
	glTexCoord2f(0.0f, 1.0f);
	glVertex3f(0, height, 0.0f);
	glEnd();
}
void draw() {
	drawKinectData();
	glutSwapBuffers();
}

void execute() {
	glutMainLoop();
}

bool initKinect() {
	// Get a working kinect sensor
	int numSensors;
	if (NuiGetSensorCount(&numSensors) < 0 || numSensors < 1) return false;
	if (NuiCreateSensorByIndex(0, &sensor) < 0) return false;

	// Initialize sensor
	sensor->NuiInitialize(NUI_INITIALIZE_FLAG_USES_DEPTH | NUI_INITIALIZE_FLAG_USES_COLOR);
	sensor->NuiImageStreamOpen(
		NUI_IMAGE_TYPE_COLOR,            // Depth camera or rgb camera?
		NUI_IMAGE_RESOLUTION_640x480,    // Image resolution
		0,      // Image stream flags, e.g. near mode
		2,      // Number of frames to buffer
		NULL,   // Event handle
		&rgbStream);

	// changing camera configuration
	sensor->NuiCameraElevationSetAngle(0);

	return sensor;
}

bool initKinectDepth() {
	// Get a working kinect sensor
	int numSensors;
	if (NuiGetSensorCount(&numSensors) < 0 || numSensors < 1) return false;
	if (NuiCreateSensorByIndex(0, &sensor) < 0) return false;

	// Initialize sensor
	sensor->NuiInitialize(NUI_INITIALIZE_FLAG_USES_DEPTH | NUI_INITIALIZE_FLAG_USES_COLOR);

	// --------------- START CHANGED CODE -----------------
	sensor->NuiImageStreamOpen(
		NUI_IMAGE_TYPE_DEPTH,                     // Depth camera or rgb camera?
		NUI_IMAGE_RESOLUTION_640x480,             // Image resolution
		NUI_IMAGE_STREAM_FLAG_ENABLE_NEAR_MODE,   // Image stream flags, e.g. near mode
		2,      // Number of frames to buffer
		NULL,   // Event handle
		&depthStream);
	// --------------- END CHANGED CODE -----------------
	return sensor;
}

bool init(int argc, char* argv[]) {
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_DEPTH | GLUT_DOUBLE | GLUT_RGBA);
	glutInitWindowSize(width, height);
	glutCreateWindow("Kinect SDK Tutorial");
	glutDisplayFunc(draw);
	glutIdleFunc(draw);
	return true;
}

// display in OpenGL
int main(int argc, char* argv[]) {
	if (!init(argc, argv)) return 1;
	if (!initKinect()) return 1;

	/* ...OpenGL texture and camera initialization... */
	// Initialize textures
	glGenTextures(1, &textureId);
	glBindTexture(GL_TEXTURE_2D, textureId);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, width, height,
		0, GL_BGRA_EXT, GL_UNSIGNED_BYTE, (GLvoid*)data);
	glBindTexture(GL_TEXTURE_2D, 0);

	// OpenGL setup
	glClearColor(0, 0, 0, 0);
	glClearDepth(1.0f);
	glEnable(GL_TEXTURE_2D);

	// Camera setup
	glViewport(0, 0, width, height);
	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();
	glOrtho(0, width, height, 0, 1, -1);
	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();

	// Main loop
	execute();
	return 0;
}

