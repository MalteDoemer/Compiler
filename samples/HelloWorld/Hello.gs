func main() {

    
    let inp = prompt("Fett? ")
    println(inp)

    switch inp {
        case "ja":
            print("saik")
        case "ne":
            print("saik")
        default: 
            print("fett")
    }

    input()
}


func prompt(text: str): str{
    print(text)
    return input()
}