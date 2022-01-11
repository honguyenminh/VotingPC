#pragma once

// Shared signals
// App closed
#define CLOSE 'C'
// Start scanning
#define START_SCAN 'S'

// VotingPC send signals
// Acknowledge
#define V_ACK 'V'
// Valid finger
#define V_FOUND 'G'
// Invalid finer
#define V_INVALID 'I'
// VotingPC receive signals
// Delete finger
#define V_DELETE_FINGER 'K'

// FingerGet signals
#define F_ACK 'F'