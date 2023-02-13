// Loads the tick audio sound in to an audio object.
let audio = new Audio('wheel/tick.mp3');

// This function is called when the sound is to be played.
function playSound() {
    // Stop and rewind the sound if it already happens to be playing.
    audio.pause();
    audio.currentTime = 0;

    // Play the sound.
    audio.play();
}

// Vars used by the code in this page to do power controls.
let wheelPower = 0;
let wheelSpinning = false;

// -------------------------------------------------------
// Click handler for spin button.
// -------------------------------------------------------
function startSpin() {
    // Ensure that spinning can't be clicked again while already running.
    if (wheelSpinning == false)
    {
        // Based on the power level selected adjust the number of spins for the wheel, the more times is has
        // to rotate with the duration of the animation the quicker the wheel spins.
        if (wheelPower == 1)
        {
            theWheel.animation.spins = 3;
        }
        else if (wheelPower == 2)
        {
            theWheel.animation.spins = 6;
        }
        else if (wheelPower == 3)
        {
            theWheel.animation.spins = 10;
        }

        // Disable the spin button so can't click again while wheel is spinning.
        document.getElementById('spin_button').src = "wheel/spin_off.png";
        document.getElementById('spin_button').className = "";

        // Begin the spin animation by calling startAnimation on the wheel object.
        theWheel.startAnimation();

        // Set to true so that power can't be changed and spin button re-enabled during
        // the current animation. The user will have to reset before spinning again.
        wheelSpinning = true;
    }
}

// -------------------------------------------------------
// Called when the spin animation has finished by the callback feature of the wheel because I specified callback in the parameters.
// -------------------------------------------------------
function alertPrize(indicatedSegment) {
    DotNet.invokeMethodAsync('ShopWeb', 'WheelFinished', indicatedSegment.text);
    /*if (indicatedSegment.text == 'BANKRUPT') {
        alert('Oh no, you have gone BANKRUPT!');
    } else {
        alert("You have won " + indicatedSegment.text);
    }*/
}

// Create new wheel object specifying the parameters at creation time.
window.theWheel = new Winwheel({
    'outerRadius': 212,        // Set outer radius so wheel fits inside the background.
    'innerRadius': 75,         // Make wheel hollow so segments don't go all way to center.
    'textFontSize': 15,         // Set default font size for the segments.
    'textOrientation': 'vertical', // Make text vertial so goes down from the outside of wheel.
    'textAlignment': 'outer',    // Align text to outside of wheel.
    'numSegments': segments.length,         // Specify number of segments.
    'segments': segments,
    'animation':           // Specify the animation to use.
    {
        'type': 'spinToStop',
        'duration': 10,    // Duration in seconds.
        'spins': 3,     // Default number of complete spins.
        'callbackFinished': window.alertPrize,
        'callbackSound': window.playSound,   // Function to call when the tick sound is to be triggered.
        'soundTrigger': 'pin'        // Specify pins are to trigger the sound, the other option is 'segment'.
    },
    'pins':				// Turn pins on.
    {
        'number': segments.length,
        'fillStyle': 'silver',
        'outerRadius': 5,
        'responsive': true
    },
    'responsive': true
})