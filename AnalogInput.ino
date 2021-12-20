int sensorPin = A7;

void setup() {
  Serial.begin(115200);
}

void loop() {
  Serial.println(analogRead(sensorPin));
}
