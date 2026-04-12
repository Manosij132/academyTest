var i = 0;

function timedCount() {
  i = i + 1;
  postMessage(`web worker count: ${i}`);
  setTimeout(() => {
    timedCount();
  }, 10000);
}

timedCount();
