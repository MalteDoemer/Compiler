func main() {
    
    let inp = prompt("Fett? ")
    println(inp)

    input()
}


func prompt(text: str): str{
    print(text)
    return input()
}