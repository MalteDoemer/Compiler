def main() {
    var name = prompt("What's your name?")
    print("Hello " + name)
}

def prompt(message: str): str {
    print(message)
    return input()
}