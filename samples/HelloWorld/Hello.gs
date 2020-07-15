def main() {
    
    while true {
        var inp = prompt("Enter your name")
        if inp == "cls"{
            clear()
            continue
        }
        else if inp == "exit"
            return void
        else if inp == "fuck you" || inp == "Fuck you"
            print("Fuck you to!")
        else
            print("Hello " + inp + "!")
        
        print("")
    }
}

def prompt(message: str): str {
    print(message)
    return input()
}
